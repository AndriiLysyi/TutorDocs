using TutorDocs.Shared.Models.Enums;

namespace TutorDocs.Shared.Models.Dto;

public sealed record DocumentWithMetadataDto
{
    public required Guid Id { get; set; }
    public required string OriginalFilename { get; set; }
    public required string FileHash { get; set; }
    public required DocumentStatus Status { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public required string DisplayTitle { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }
    public IEnumerable<string> Tags { get; set; } = [];
    public string? Notes { get; set; }
    public required bool IsOwner { get; set; }
}