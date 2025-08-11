using Microsoft.AspNetCore.Http;

namespace TutorDocs.Shared.Models.Requests;

public sealed record UploadDocumentRequest
{
    public IFormFile? File { get; set; } = null!;
    public string? Author { get; set; }
    public IReadOnlyList<string> Tags { get; set; } = [];
    public string? Notes { get; set; }
    public string? DisplayTitle { get; set; }
    public string? Description { get; set; }
}