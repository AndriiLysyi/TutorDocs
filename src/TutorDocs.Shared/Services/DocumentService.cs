using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using TutorDocs.Shared.Extensions;
using TutorDocs.Shared.Models.Dto;
using TutorDocs.Shared.Models.Requests;
using TutorDocs.Shared.Models.Responses;
using TutorDocs.Shared.Repositories;

namespace TutorDocs.Shared.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(IDocumentRepository documentRepository, ILogger<DocumentService> logger)
    {
        _documentRepository = documentRepository;
        _logger = logger;
    }

    public async Task<UploadDocumentResponse> CreateDocumentAsync(UploadDocumentRequest request, Guid userId)
    {
        try
        {
            await using var stream = request.File?.OpenReadStream();
            var fileHash = await ComputeFileHashAsync(stream!);
            
            var existingDocument = await _documentRepository.GetDocumentByHashAsync(fileHash);

            if (existingDocument != null)
            {
                return await AddOwnerAsync(request, existingDocument.Id, userId);
            }
            
            var createdDocumentId = await CreateDocumentInternal(request, fileHash, userId);

            return new UploadDocumentResponse
            {
                DocumentId = createdDocumentId,
                Message = "Document uploaded successfully",
                IsSuccess = true,
                WasExistingFile = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document for user {UserId}", userId);
            return new UploadDocumentResponse
            {
                DocumentId = Guid.Empty,
                Message = "Error uploading document",
                IsSuccess = false,
                WasExistingFile = false
            };
        }
    }

    public async Task<DocumentWithMetadataDto?> GetDocumentAsync(Guid documentId, Guid userId)
    {
        return await _documentRepository.GetDocumentWithMetadataAsync(documentId, userId);
    }

    public async Task<IEnumerable<DocumentWithMetadataDto>> GetUserDocumentsAsync(Guid userId)
    {
        return await _documentRepository.GetUserDocumentsAsync(userId);
    }

    public async Task<DeleteDocumentResponse> DeleteDocumentAsync(Guid documentId, Guid userId)
    {
        var success = await _documentRepository.DeleteDocumentAsync(documentId, userId);
        if (success)
        {
            await _documentRepository.SaveChanges();    
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
        var hasOwnership = await _documentRepository.HasDocumentOwnershipAsync(id, userId);

        if (hasOwnership)
        {
            return new UploadDocumentResponse
            {
                DocumentId = id,
                Message = "Document already exists in your library",
                IsSuccess = true,
                WasExistingFile = true
            };
        }
                
        var metadata = request.MapToDocumentOwner(id, userId).Metadata;
        _documentRepository.AddDocumentOwnershipAsync(id, userId, metadata);
        await _documentRepository.SaveChanges();
        
        return new UploadDocumentResponse
        {
            DocumentId = id,
            Message = "Document added to your library",
            IsSuccess = true,
            WasExistingFile = true
        };
    }
    
    private async Task<Guid> CreateDocumentInternal(UploadDocumentRequest request, string fileHash, Guid userId)
    {
        var documentDto = request!.MapToDocument().MapToDocumentDto();
        documentDto.FileHash = fileHash;
            
        var createdDocument = _documentRepository.CreateDocumentAsync(documentDto);
            
        var ownerMetadata = request.MapToDocumentOwner(createdDocument.Id, userId).Metadata;
        _documentRepository.AddDocumentOwnershipAsync(createdDocument.Id, userId, ownerMetadata);
        await _documentRepository.SaveChanges();
        
        _logger.LogInformation("Document {DocumentId} created successfully for user {UserId}", createdDocument.Id, userId);
        
        return createdDocument.Id;
    }
}