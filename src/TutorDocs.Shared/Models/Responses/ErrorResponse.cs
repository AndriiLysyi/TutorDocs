namespace TutorDocs.Shared.Models.Responses;

public sealed record ErrorResponse
{
    public required string Message { get; init; }
    public string? Details { get; init; }
    public required int StatusCode { get; init; }
}