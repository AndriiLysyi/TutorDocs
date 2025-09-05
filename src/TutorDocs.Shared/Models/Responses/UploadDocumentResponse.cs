namespace TutorDocs.Shared.Models.Responses;

public sealed record UploadDocumentResponse
{
    public Guid DocumentId { get; init; }
    public string Message { get; init; } = string.Empty;
    public bool IsSuccess { get; init; }
    public bool WasExistingFile { get; init; }
}