namespace TutorDocs.Shared.Models.Responses;

public sealed record DeleteDocumentResponse
{
    public required bool IsSuccess { get; init; }
    public required string Message { get; init; }
}