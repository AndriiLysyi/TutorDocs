#!/bin/bash

# TutorDocs Development Environment Setup Script
# This script sets up the complete development environment with LocalStack S3

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
PROJECT_NAME="TutorDocs"
API_PROJECT_PATH="src/TutorDocs.Api"
SHARED_PROJECT_PATH="src/TutorDocs.Shared"
DOCKER_DATA_DIR="./docker-data"
S3_BUCKET_NAME="tutordocs-documents"

# Logging functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Wait for service to be ready with timeout
wait_for_service() {
    local service_name=$1
    local health_command=$2
    local max_attempts=${3:-30}
    local attempt=1
    
    log_info "Waiting for $service_name to be ready..."
    
    while [ $attempt -le $max_attempts ]; do
        if eval "$health_command" > /dev/null 2>&1; then
            log_success "$service_name is ready!"
            return 0
        fi
        
        echo -n "."
        sleep 2
        attempt=$((attempt + 1))
    done
    
    log_error "$service_name failed to start within $((max_attempts * 2)) seconds"
    return 1
}

# Step 1: Check system dependencies
check_dependencies() {
    log_info "Checking system dependencies..."
    
    local missing_deps=()
    
    if ! command_exists docker; then
        missing_deps+=("docker")
    fi
    
    if ! command_exists docker-compose; then
        missing_deps+=("docker-compose")
    fi
    
    if ! command_exists dotnet; then
        missing_deps+=("dotnet")
    fi
    
    if ! command_exists curl; then
        missing_deps+=("curl")
    fi
    
    if [ ${#missing_deps[@]} -ne 0 ]; then
        log_error "Missing required dependencies: ${missing_deps[*]}"
        log_info "Please install the missing dependencies and run this script again"
        exit 1
    fi
    
    # Check .NET version (require 9+)
    local dotnet_version=$(dotnet --version | cut -d'.' -f1)
    if [ "$dotnet_version" -lt 9 ]; then
        log_error ".NET version $(dotnet --version) detected. .NET 9+ is required"
        exit 1
    fi
    
    # Check Docker daemon
    if ! docker info >/dev/null 2>&1; then
        log_error "Docker daemon is not running. Please start Docker and try again"
        exit 1
    fi
    
    log_success "All dependencies are available (dotnet $(dotnet --version))"
}

# Step 2: Create necessary directories
create_directories() {
    log_info "Creating necessary directories..."
    
    # Create docker data directories only if they don't exist
    if [ ! -d "$DOCKER_DATA_DIR" ]; then
        mkdir -p "$DOCKER_DATA_DIR/postgres"
        mkdir -p "$DOCKER_DATA_DIR/localstack"
        log_success "Created $DOCKER_DATA_DIR directories"
    else
        log_info "$DOCKER_DATA_DIR already exists, skipping creation"
    fi
}

# Step 3: Create .env file for Docker containers
create_env_file() {
    log_info "Creating/updating .env file for Docker..."
    
    cat > .env << 'EOF'
# Database Configuration
DB_NAME=tutordocs
DB_USER=postgres
DB_PASSWORD=postgres

# API Configuration
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Host=postgresql;Database=tutordocs;Username=postgres;Password=postgres

# LocalStack S3 Configuration
LOCALSTACK_SERVICES=s3
AWS__ServiceURL=http://localstack:4566
AWS__AccessKey=test
AWS__SecretKey=test
AWS__BucketName=tutordocs-documents
AWS__Region=eu-central-1
EOF
    
    log_success ".env file created/updated"
}

# Step 4: Build .NET solution
build_solution() {
    log_info "Building .NET solution..."
    
    if ! dotnet build --configuration Release; then
        log_error "Failed to build .NET solution"
        exit 1
    fi
    
    log_success ".NET solution built successfully"
}

# Step 4a: Clean up old Docker images and build fresh API container
cleanup_and_build_api() {
    log_info "Cleaning up old API containers and images..."
    
    # Stop and remove existing API container
    docker-compose stop tutordocs_api 2>/dev/null || true
    docker-compose rm -f tutordocs_api 2>/dev/null || true
    
    # Remove old API images to force rebuild
    docker images --format "table {{.Repository}}\t{{.Tag}}\t{{.ID}}" | grep "tutordocs.*api" | awk '{print $3}' | xargs -r docker rmi -f 2>/dev/null || true
    
    # Clean up dangling images and build cache
    docker image prune -f 2>/dev/null || true
    docker builder prune -f 2>/dev/null || true
    
    log_success "Old API containers and images cleaned up"
    
    log_info "Building fresh API container with latest code..."
    
    # Build API container from scratch
    if ! docker-compose build --no-cache tutordocs_api; then
        log_error "Failed to build API container"
        exit 1
    fi
    
    log_success "Fresh API container built successfully"
}

# Step 5: Start Docker infrastructure services
start_infrastructure_services() {
    log_info "Starting Docker infrastructure services..."
    
    # Stop any existing services
    docker-compose down --remove-orphans 2>/dev/null || true
    
    # Start only infrastructure services first
    if ! docker-compose up -d postgresql localstack; then
        log_error "Failed to start infrastructure services"
        exit 1
    fi
    
    log_success "Infrastructure services started"
}

# Step 5a: Start API service
start_api_service() {
    log_info "Starting API service with fresh container..."
    
    # Start the freshly built API container
    if ! docker-compose up -d tutordocs_api; then
        log_error "Failed to start API service"
        exit 1
    fi
    
    log_success "API service started"
}

# Step 5b: Wait for API service to be healthy
wait_for_api_health() {
    log_info "Waiting for API service to be ready..."
    
    # Wait for API health check with longer timeout for first startup
    wait_for_service "TutorDocs API" "curl -s http://localhost:8080/healthz | grep -q 'Healthy'" 60
    
    log_success "API service is healthy and ready"
}

# Step 6: Wait for services to be healthy
wait_for_health() {
    log_info "Waiting for services to be ready..."
    
    # Wait for PostgreSQL
    wait_for_service "PostgreSQL" "docker-compose exec -T postgresql pg_isready -U tutordocs_user" 30
    
    # Wait for LocalStack S3
    wait_for_service "LocalStack S3" "curl -s http://localhost:4566/_localstack/health | grep -qE 's3.*(available|running)'" 30
    
    log_success "All services are healthy"
}

# Step 7: Run database migrations
run_migrations() {
    log_info "Running database migrations..."
    
    cd "$SHARED_PROJECT_PATH"
    
    if ! dotnet ef database update --startup-project "../TutorDocs.Api"; then
        log_error "Failed to run database migrations"
        exit 1
    fi
    
    cd - > /dev/null
    
    log_success "Database migrations completed"
}

# Step 8: Setup LocalStack S3 buckets using docker exec
setup_s3_buckets() {
    log_info "Setting up LocalStack S3 buckets using docker commands..."
    
    # Create S3 bucket using docker exec with awslocal
    log_info "Creating S3 bucket '$S3_BUCKET_NAME'..."
    if docker-compose exec -T localstack awslocal s3 mb "s3://$S3_BUCKET_NAME" --region eu-central-1; then
        log_success "S3 bucket '$S3_BUCKET_NAME' created successfully"
    else
        # Check if bucket already exists
        if docker-compose exec -T localstack awslocal s3 ls | grep -q "$S3_BUCKET_NAME"; then
            log_warning "S3 bucket '$S3_BUCKET_NAME' already exists"
        else
            log_error "Failed to create S3 bucket '$S3_BUCKET_NAME'"
            exit 1
        fi
    fi
    
    # List buckets to verify
    log_info "Verifying S3 buckets..."
    if docker-compose exec -T localstack awslocal s3 ls; then
        log_success "S3 bucket verification completed"
    else
        log_warning "Could not list S3 buckets for verification"
    fi
    
    # Set bucket CORS configuration for signed URLs
    log_info "Configuring S3 bucket CORS policy..."
    docker-compose exec -T localstack awslocal s3api put-bucket-cors \
        --bucket "$S3_BUCKET_NAME" \
        --cors-configuration '{
            "CORSRules": [
                {
                    "AllowedHeaders": ["*"],
                    "AllowedMethods": ["GET", "PUT", "POST", "DELETE"],
                    "AllowedOrigins": ["*"],
                    "ExposeHeaders": ["ETag"],
                    "MaxAgeSeconds": 3000
                }
            ]
        }' && log_success "CORS policy configured" || log_warning "CORS policy configuration failed"
}

# Step 9: Verify environment
verify_environment() {
    log_info "Verifying development environment..."
    
    local verification_failed=false
    
    # Check PostgreSQL connection
    if docker-compose exec -T postgresql pg_isready -U tutordocs_user > /dev/null 2>&1; then
        log_success "PostgreSQL is accessible"
    else
        log_error "PostgreSQL is not accessible"
        verification_failed=true
    fi
    
    # Check LocalStack S3 health
    if curl -s "http://localhost:4566/_localstack/health" | grep -q "s3.*running" 2>/dev/null; then
        log_success "LocalStack S3 is running and healthy"
    else
        log_error "LocalStack S3 is not accessible or unhealthy"
        verification_failed=true
    fi
    
    # Check if S3 bucket exists
    if docker-compose exec -T localstack awslocal s3 ls | grep -q "$S3_BUCKET_NAME" 2>/dev/null; then
        log_success "S3 bucket '$S3_BUCKET_NAME' is accessible"
    else
        log_warning "S3 bucket verification failed"
    fi
    
    if [ "$verification_failed" = true ]; then
        log_error "Environment verification failed"
        exit 1
    fi
    
    log_success "Environment verification completed successfully"
}

# Step 10: Display final status and next steps
display_status() {
    echo ""
    echo "=============================================="
    log_success "🎉 Development environment is ready!"
    echo "=============================================="
    echo ""
    log_info "Services Status:"
    echo "  • PostgreSQL: ✓ Running on port 5432"
    echo "  • LocalStack S3: ✓ Running on port 4566"
    echo "  • TutorDocs API: ✓ Running on port 8080 (containerized with latest code)"
    echo "  • S3 Bucket: ✓ '$S3_BUCKET_NAME' created and accessible"
    echo "  • Region: ✓ eu-central-1 (Europe/Ukraine)"
    echo ""
    log_info "API Endpoints:"
    echo "  • Swagger UI: http://localhost:8080/openapi"
    echo "  • Health Check: http://localhost:8080/healthz"
    echo "  • Detailed Health: http://localhost:8080/healthz/ready"
    echo "  • Database Test: http://localhost:8080/api/test/db"
    echo ""
    log_info "Development Options:"
    echo "  1. Use containerized API (recommended): Already running on port 8080"
    echo "  2. Run API locally for debugging: cd $API_PROJECT_PATH && dotnet run"
    echo "     (Local API will run on port 5147 for development/debugging)"
    echo ""
    log_info "LocalStack S3 Information:"
    echo "  • Health Endpoint: http://localhost:4566/health"
    echo "  • S3 Management: docker-compose exec localstack awslocal s3 ls"
    echo "  • Create signed URLs: Available via API endpoints"
    echo ""
    log_info "Useful Commands:"
    echo "  • Stop services: docker-compose down"
    echo "  • View logs: docker-compose logs localstack"
    echo "  • S3 operations: docker-compose exec localstack awslocal s3 [command]"
    echo ""
    log_info "Note: Configure your .NET user secrets for local API development"
}

# Main execution flow
main() {
    echo "=============================================="
    echo "    $PROJECT_NAME Development Setup"
    echo "=============================================="
    echo ""
    
    check_dependencies
    create_directories
    create_env_file
    build_solution
    start_infrastructure_services
    wait_for_health
    run_migrations
    setup_s3_buckets
    cleanup_and_build_api
    start_api_service
    wait_for_api_health
    verify_environment
    display_status
}

# Execute main function
main "$@"