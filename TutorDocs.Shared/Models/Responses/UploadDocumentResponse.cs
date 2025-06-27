namespace TutorDocs.Shared.Models.Responses;

public class UploadDocumentResponse
{
    public Guid DocumentId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public bool WasExistingFile { get; set; }
}