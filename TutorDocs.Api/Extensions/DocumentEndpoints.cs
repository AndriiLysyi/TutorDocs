using Microsoft.AspNetCore.Mvc;
using TutorDocs.Shared.Models.Requests;
using TutorDocs.Shared.Models.Responses;
using TutorDocs.Shared.Services;

namespace TutorDocs.Api.Extensions;

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
            .Produces<UploadDocumentResponse>(200)
            .Produces<object>(400);

        group.MapGet("/", GetUserDocuments)
            .WithName("GetUserDocuments")
            .WithSummary("Get all documents for the current user")
            .Produces<IEnumerable<object>>(200);

        group.MapGet("/{id:guid}", GetDocument)
            .WithName("GetDocument")
            .WithSummary("Get a specific document by ID")
            .Produces<object>(200)
            .Produces<object>(404);

        group.MapDelete("/{id:guid}", DeleteDocument)
            .WithName("DeleteDocument")
            .WithSummary("Delete a document")
            .Produces<object>(200)
            .Produces<object>(404);
    }

    private static async Task<IResult> UploadDocument(
        [FromForm] UploadDocumentRequest request,
        IDocumentService documentService,
        ILogger<Program> logger)
    {
        try
        {
            if (request.File == null || request.File.Length == 0)
            {
                return TypedResults.BadRequest(new { Message = "No file provided" });
            }

            // TODO : remove hardcoded user
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            var response = await documentService.CreateDocumentAsync(request, userId);

            if (response.IsSuccess)
            {
                return TypedResults.Ok(response);
            }

            return TypedResults.BadRequest(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading document");
            return TypedResults.Problem("Internal server error", statusCode: 500);
        }
    }

    private static async Task<IResult> GetUserDocuments(
        IDocumentService documentService,
        ILogger<Program> logger)
    {
        try
        {
            // TODO : remove hardcoded user
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            var documents = await documentService.GetUserDocumentsAsync(userId);
            return TypedResults.Ok(documents);
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
        ILogger<Program> logger)
    {
        try
        {
            // TODO : remove hardcoded user
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            var document = await documentService.GetDocumentAsync(id, userId);
            
            if (document == null)
            {
                return TypedResults.NotFound(new { Message = "Document not found or access denied" });
            }

            return TypedResults.Ok(document);
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
        ILogger<Program> logger)
    {
        try
        {
            // TODO : remove hardcoded user
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            var success = await documentService.DeleteDocumentAsync(id, userId);
            
            if (!success)
            {
                return TypedResults.NotFound(new { Message = "Document not found or access denied" });
            }

            return TypedResults.Ok(new { Message = "Document deleted successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting document {DocumentId}", id);
            return TypedResults.Problem("Internal server error", statusCode: 500);
        }
    }
}