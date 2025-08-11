namespace TutorDocs.Shared.Models.Responses;

public sealed record ErrorResponse
{
    public required string Message { get; set; }
    public string? Details { get; set; }
    public required int StatusCode { get; set; }
}