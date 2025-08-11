namespace TutorDocs.Shared.Models.Responses;

public sealed record DeleteDocumentResponse
{
    public required bool IsSuccess { get; set; }
    public required string Message { get; set; }
}