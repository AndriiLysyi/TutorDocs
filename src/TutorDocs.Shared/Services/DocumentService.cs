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
            
            var existingDocument = await _documentRepository.GetDocumentByHash(fileHash);

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
        return await _documentRepository.GetDocumentWithMetadata(documentId, userId);
    }

    public async Task<IEnumerable<DocumentWithMetadataDto>> GetUserDocumentsAsync(Guid userId)
    {
        return await _documentRepository.GetUserDocuments(userId);
    }

    public async Task<DeleteDocumentResponse> DeleteDocumentAsync(Guid documentId, Guid userId)
    {
        var success = await _documentRepository.DeleteDocument(documentId, userId);
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
        await _documentRepository.BeginTransaction();
        var hasOwnership = await _documentRepository.HasDocumentOwnership(id, userId);

        if (hasOwnership)
        {
            return ContractMapping.MapToUploadDocumentResponse(id, 
                "Document already exists in your library", 
                true);
        }
                
        var metadata = request.MapToDocumentOwner(id, userId).Metadata;
        _documentRepository.AddDocumentOwnership(id, userId, metadata);
        await _documentRepository.SaveChanges();
        await _documentRepository.CommitTransaction();

        return ContractMapping.MapToUploadDocumentResponse(id, 
            "Document added to your library", 
            true);
    }
    
    private async Task<Guid> CreateDocumentInternal(UploadDocumentRequest request, string fileHash, Guid userId)
    {
        var documentDto = request.MapToDocumentDto(fileHash);
        
        await _documentRepository.BeginTransaction();
        var createdDocument = _documentRepository.CreateDocument(documentDto);
            
        var ownerMetadata = request.MapToDocumentOwner(createdDocument.Id, userId).Metadata;
        _documentRepository.AddDocumentOwnership(createdDocument.Id, userId, ownerMetadata);
        await _documentRepository.SaveChanges();
        await _documentRepository.CommitTransaction();
        
        _logger.LogInformation("Document {DocumentId} created successfully for user {UserId}", createdDocument.Id, userId);
        
        return createdDocument.Id;
    }
}