namespace TutorDocs.Shared.Models.Responses;

public class GetUserDocumentsResponse
{
    public required IEnumerable<DocumentResponse> Documents { get; set; } = [];
}