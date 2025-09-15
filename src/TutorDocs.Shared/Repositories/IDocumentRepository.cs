using TutorDocs.Shared.Data.Entities;
using TutorDocs.Shared.Models.Dto;

namespace TutorDocs.Shared.Repositories;

public interface IDocumentRepository
{
    Task<DocumentDto?> GetDocumentByIdAsync(Guid documentId);
    Task<DocumentWithMetadataDto?> GetDocumentWithMetadata(Guid documentId, Guid userId);
    Task<IEnumerable<DocumentWithMetadataDto>> GetUserDocuments(Guid userId);
    Task<DocumentDto?> GetDocumentByHash(string fileHash);
    DocumentDto CreateDocument(DocumentDto document);
    Task<bool> DeleteDocument(Guid documentId, Guid userId);
    Task<bool> HasDocumentOwnership(Guid documentId, Guid userId);
    void AddDocumentOwnership(Guid documentId, Guid userId, DocumentMetadata metadata);
}