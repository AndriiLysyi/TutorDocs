using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TutorDocs.Shared.Extensions;
using TutorDocs.Shared.Models.Requests;
using TutorDocs.Shared.Models.Responses;
using TutorDocs.Shared.Services;

namespace TutorDocs.Api.Endpoints;

public static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/documents")
            .WithTags("Documents")
            .WithOpenApi();

        group.MapPost("/upload", UploadDocument)
            .WithName("UploadDocument")
            .WithSummary("Upload a document with metadata")
            .Accepts<UploadDocumentRequest>("multipart/form-data")
            .DisableAntiforgery()
            .Produces<UploadDocumentResponse>(StatusCodes.Status200OK)
            .Produces<UploadDocumentResponse>(StatusCodes.Status400BadRequest);

        group.MapGet("/", GetUserDocuments)
            .WithName("GetUserDocuments")
            .WithSummary("Get all documents for the current user")
            .Produces<GetUserDocumentsResponse>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetDocument)
            .WithName("GetDocument")
            .WithSummary("Get a specific document by ID")
            .Produces<GetDocumentResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteDocument)
            .WithName("DeleteDocument")
            .WithSummary("Delete a document")
            .Produces<DeleteDocumentResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/download-url", GetDocumentDownloadUrl)
            .WithName("GetDocumentDownloadUrl")
            .WithSummary("Get a presigned URL to download a document")
            .Produces<string>(StatusCodes.Status200OK, "text/plain")
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/stream", GetDocumentStream)
            .WithName("GetDocumentStream")
            .WithSummary("Download document as stream")
            .Produces<FileStreamHttpResult>(StatusCodes.Status200OK, "application/octet-stream")
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> UploadDocument(
        [FromForm] UploadDocumentRequest request,
        IDocumentService documentService,
        IUserProvider userProvider,
        ILogger<Program> logger)
    {
        try
        {
            if (request.File == null || request.File.Length == 0)
            {
                return TypedResults.BadRequest(new UploadDocumentResponse
                {
                    Message = "No file provided",
                    IsSuccess = false
                });
            }

            var userId = await userProvider.GetCurrentUserIdAsync();
            var response = await documentService.CreateDocumentAsync(request, userId);

            return response.IsSuccess 
                ? TypedResults.Ok(response) 
                : TypedResults.BadRequest(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading document");
            return TypedResults.Problem("Internal server error", statusCode: 500);
        }
    }

    private static async Task<IResult> GetUserDocuments(
        IDocumentService documentService,
        IUserProvider userProvider,
        ILogger<Program> logger)
    {
        try
        {
            var userId = await userProvider.GetCurrentUserIdAsync();
            var documents = await documentService.GetUserDocumentsAsync(userId);
            var response = documents.MapToGetUserDocumentsResponse();
            return TypedResults.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user documents");
            return TypedResults.Problem("Internal server error", statusCode: 500);
        }
    }

    private static async Task<IResult> GetDocument(
        Guid id,
        IDocumentService documentService,
        IUserProvider userProvider,
        ILogger<Program> logger)
    {
        try
        {
            var userId = await userProvider.GetCurrentUserIdAsync();
            var document = await documentService.GetDocumentAsync(id, userId);
            
            if (document == null)
            {
                var errorResponse = ContractMapping.MapToErrorResponse("Document not found or access denied", 404);
                return TypedResults.NotFound(errorResponse);
            }

            var response = document.MapToGetDocumentResponse();
            return TypedResults.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving document {DocumentId}", id);
            return TypedResults.Problem("Internal server error", statusCode: 500);
        }
    }

    private static async Task<IResult> DeleteDocument(
        Guid id,
        IDocumentService documentService,
        IUserProvider userProvider,
        ILogger<Program> logger)
    {
        try
        {
            var userId = await userProvider.GetCurrentUserIdAsync();
            var response = await documentService.DeleteDocumentAsync(id, userId);
            
            if (!response.IsSuccess)
            {
                var errorResponse = ContractMapping.MapToErrorResponse(response.Message, 404);
                return TypedResults.NotFound(errorResponse);
            }

            return TypedResults.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting document {DocumentId}", id);
            return TypedResults.Problem("Internal server error", statusCode: 500);
        }
    }

    private static async Task<IResult> GetDocumentDownloadUrl(
        Guid id,
        IDocumentService documentService,
        IUserProvider userProvider,
        ILogger<Program> logger,
        int expirationHours = 1)
    {
        try
        {
            var userId = await userProvider.GetCurrentUserIdAsync();
            var expiration = TimeSpan.FromHours(expirationHours);
            var downloadUrl = await documentService.GetDocumentDownloadUrl(id, userId, expiration);
            
            return TypedResults.Ok(downloadUrl);
        }
        catch (UnauthorizedAccessException)
        {
            var errorResponse = ContractMapping.MapToErrorResponse("Document not found or access denied", 404);
            return TypedResults.NotFound(errorResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating download URL for document {DocumentId}", id);
            return TypedResults.Problem("Internal server error", statusCode: 500);
        }
    }

    private static async Task<IResult> GetDocumentStream(
        Guid id,
        IDocumentService documentService,
        IUserProvider userProvider,
        ILogger<Program> logger)
    {
        try
        {
            var userId = await userProvider.GetCurrentUserIdAsync();
            
            // Get document metadata first to get the original filename
            var documentMetadata = await documentService.GetDocumentAsync(id, userId);
            if (documentMetadata == null)
            {
                var errorResponse = ContractMapping.MapToErrorResponse("Document not found or access denied", 404);
                return TypedResults.NotFound(errorResponse);
            }

            var stream = await documentService.GetDocumentStream(id, userId);
            var contentType = "application/octet-stream";
            var fileName = documentMetadata.OriginalFilename;
            
            return TypedResults.Stream(stream, contentType, fileName);
        }
        catch (UnauthorizedAccessException)
        {
            var errorResponse = ContractMapping.MapToErrorResponse("Document not found or access denied", 404);
            return TypedResults.NotFound(errorResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error streaming document {DocumentId}", id);
            return TypedResults.Problem("Internal server error", statusCode: 500);
        }
    }
}