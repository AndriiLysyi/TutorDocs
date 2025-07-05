namespace TutorDocs.Shared.Models.Responses;

public class DeleteDocumentResponse
{
    public required bool IsSuccess { get; set; }
    public required string Message { get; set; }
}