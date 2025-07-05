namespace TutorDocs.Shared.Models.Responses;

public class ErrorResponse
{
    public required string Message { get; set; }
    public string? Details { get; set; }
    public required int StatusCode { get; set; }
}