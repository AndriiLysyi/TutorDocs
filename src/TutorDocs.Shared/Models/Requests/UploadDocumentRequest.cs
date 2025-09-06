using Microsoft.AspNetCore.Http;

namespace TutorDocs.Shared.Models.Requests;

public sealed record UploadDocumentRequest
{
    public required IFormFile File { get; init; }
    public string? Author { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public string? Notes { get; init; }
    public string? DisplayTitle { get; init; }
    public string? Description { get; init; }
}