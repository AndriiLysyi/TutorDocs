using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using TutorDocs.Shared.Extensions;
using TutorDocs.Shared.Models.Dto;
using TutorDocs.Shared.Models.Requests;
using TutorDocs.Shared.Models.Responses;

namespace TutorDocs.Shared.Services;

public class DocumentService : IDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IS3Service _s3Service;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(IUnitOfWork unitOfWork, IS3Service s3Service, ILogger<DocumentService> logger)
    {
        _unitOfWork = unitOfWork;
        _s3Service = s3Service;
        _logger = logger;
    }

    public async Task<UploadDocumentResponse> CreateDocumentAsync(UploadDocumentRequest request, Guid userId)
    {
        try
        {
            await using var stream = request.File?.OpenReadStream();
            var fileHash = await ComputeFileHashAsync(stream!);
            
            var existingDocument = await _unitOfWork.DocumentRepository.GetDocumentByHash(fileHash);

            if (existingDocument != null)
            {
                return await AddOwnerAsync(request, existingDocument.Id, userId);
            }
            
            var createdDocumentId = await CreateDocumentInternal(request, fileHash, userId);

            return ContractMapping.MapToUploadDocumentResponse(createdDocumentId, 
                "Document uploaded successfully", 
                false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document for user {UserId}", userId);
            return ContractMapping.MapToUploadDocumentResponse(Guid.Empty,
                "Error uploading document",
                false,
                false);
        }
    }

    public async Task<DocumentWithMetadataDto?> GetDocumentAsync(Guid documentId, Guid userId)
    {
        return await _unitOfWork.DocumentRepository.GetDocumentWithMetadata(documentId, userId);
    }

    public async Task<IEnumerable<DocumentWithMetadataDto>> GetUserDocumentsAsync(Guid userId)
    {
        return await _unitOfWork.DocumentRepository.GetUserDocuments(userId);
    }

    public async Task<DeleteDocumentResponse> DeleteDocumentAsync(Guid documentId, Guid userId)
    {
        var success = await _unitOfWork.DocumentRepository.DeleteDocument(documentId, userId);
        if (success)
        {
            var s3Key = $"documents/{documentId}";
            await _s3Service.DeleteFile(s3Key);
            
            await _unitOfWork.SaveChanges();    
        }
        
        return success 
            ? ContractMapping.MapToDeleteResponse(true, "Document deleted successfully")
            : ContractMapping.MapToDeleteResponse(false, "Document not found or access denied");
    }

    private static async Task<string> ComputeFileHashAsync(Stream stream)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(stream);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private async Task<UploadDocumentResponse> AddOwnerAsync(UploadDocumentRequest request, Guid id, Guid userId)
    {
        await _unitOfWork.BeginTransaction();
        var hasOwnership = await _unitOfWork.DocumentRepository.HasDocumentOwnership(id, userId);

        if (hasOwnership)
        {
            return ContractMapping.MapToUploadDocumentResponse(id, 
                "Document already exists in your library", 
                true);
        }
                
        var metadata = request.MapToDocumentOwner(id, userId).Metadata;
        _unitOfWork.DocumentRepository.AddDocumentOwnership(id, userId, metadata);
        await _unitOfWork.SaveChanges();
        await _unitOfWork.CommitTransaction();

        return ContractMapping.MapToUploadDocumentResponse(id, 
            "Document added to your library", 
            true);
    }
    
    private async Task<Guid> CreateDocumentInternal(UploadDocumentRequest request, string fileHash, Guid userId)
    {
        var documentDto = request.MapToDocumentDto(fileHash);
        
        await _unitOfWork.BeginTransaction();
        
        try
        {
            var createdDocument = _unitOfWork.DocumentRepository.CreateDocument(documentDto);
            
            await using var fileStream = request.File?.OpenReadStream();
            var s3Key = $"documents/{createdDocument.Id}";
            var contentType = request.File?.ContentType ?? "application/octet-stream";
            
            await _s3Service.UploadFile(s3Key, fileStream!, contentType);
            
            var ownerMetadata = request.MapToDocumentOwner(createdDocument.Id, userId).Metadata;
            _unitOfWork.DocumentRepository.AddDocumentOwnership(createdDocument.Id, userId, ownerMetadata);
            
            await _unitOfWork.SaveChanges();
            await _unitOfWork.CommitTransaction();
            
            _logger.LogInformation("Document {DocumentId} created and uploaded to S3 successfully for user {UserId}", createdDocument.Id, userId);
            
            return createdDocument.Id;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransaction();
            _logger.LogError(ex, "Failed to create document {DocumentId} for user {UserId}", documentDto.Id, userId);
            throw;
        }
    }

    public async Task<string> GetDocumentDownloadUrl(Guid documentId, Guid userId, TimeSpan? expiration = null)
    {
        var document = await _unitOfWork.DocumentRepository.GetDocumentWithMetadata(documentId, userId);
        
        if (document == null)
        {
            throw new UnauthorizedAccessException("Document not found or access denied");
        }

        var s3Key = $"documents/{documentId}";
        var defaultExpiration = expiration ?? TimeSpan.FromHours(1);
        
        return await _s3Service.GeneratePreSignedDownloadUrl(s3Key, defaultExpiration);
    }

    public async Task<Stream> GetDocumentStream(Guid documentId, Guid userId)
    {
        var document = await _unitOfWork.DocumentRepository.GetDocumentWithMetadata(documentId, userId);
        
        if (document == null)
        {
            throw new UnauthorizedAccessException("Document not found or access denied");
        }

        var s3Key = $"documents/{documentId}";
        
        return await _s3Service.DownloadFile(s3Key);
    }
}