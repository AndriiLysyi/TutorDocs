using TutorDocs.Shared.Models.Entities;
using TutorDocs.Shared.Models.Requests;
using TutorDocs.Shared.Models.Responses;

namespace TutorDocs.Shared.Services;

public interface IDocumentService
{
    Task<UploadDocumentResponse> CreateDocumentAsync(UploadDocumentRequest request, Guid userId);
    Task<Document?> GetDocumentAsync(Guid documentId, Guid userId);
    Task<IEnumerable<Document>> GetUserDocumentsAsync(Guid userId);
    Task<bool> DeleteDocumentAsync(Guid documentId, Guid userId);
}