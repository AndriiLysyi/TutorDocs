using TutorDocs.Shared.Data.Entities;
using TutorDocs.Shared.Models.Dto;

namespace TutorDocs.Shared.Repositories;

public interface IDocumentRepository
{
    Task<DocumentDto?> GetDocumentByIdAsync(Guid documentId);
    Task<DocumentWithMetadataDto?> GetDocumentWithMetadataAsync(Guid documentId, Guid userId);
    Task<IEnumerable<DocumentWithMetadataDto>> GetUserDocumentsAsync(Guid userId);
    Task<DocumentDto?> GetDocumentByHashAsync(string fileHash);
    DocumentDto CreateDocumentAsync(DocumentDto document);
    Task<bool> DeleteDocumentAsync(Guid documentId, Guid userId);
    Task<bool> HasDocumentOwnershipAsync(Guid documentId, Guid userId);
    void AddDocumentOwnershipAsync(Guid documentId, Guid userId, DocumentMetadata metadata);
    Task<bool> HasOtherOwnersAsync(Guid documentId, Guid excludeUserId);
    Task SaveChanges();
}