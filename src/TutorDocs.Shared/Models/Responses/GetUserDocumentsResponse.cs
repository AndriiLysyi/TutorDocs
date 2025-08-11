namespace TutorDocs.Shared.Models.Responses;

public sealed record GetUserDocumentsResponse
{
    public required IEnumerable<DocumentResponse> Documents { get; set; } = [];
}