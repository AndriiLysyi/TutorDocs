using TutorDocs.Shared.Models.Dto;
using TutorDocs.Shared.Models.Requests;
using TutorDocs.Shared.Models.Responses;

namespace TutorDocs.Shared.Services;

public interface IDocumentService
{
    Task<UploadDocumentResponse> CreateDocumentAsync(UploadDocumentRequest request, Guid userId);
    Task<DocumentWithMetadataDto?> GetDocumentAsync(Guid documentId, Guid userId);
    Task<IEnumerable<DocumentWithMetadataDto>> GetUserDocumentsAsync(Guid userId);
    Task<DeleteDocumentResponse> DeleteDocumentAsync(Guid documentId, Guid userId);
    
    // S3 integration methods
    Task<string> GetDocumentDownloadUrl(Guid documentId, Guid userId, TimeSpan? expiration = null);
    Task<Stream> GetDocumentStream(Guid documentId, Guid userId);
}