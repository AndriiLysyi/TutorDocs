using TutorDocs.Shared.Models.Enums;

namespace TutorDocs.Shared.Models.Dto;

public sealed record DocumentWithMetadataDto
{
    public required Guid Id { get; init; }
    public required string OriginalFilename { get; init; }
    public required string FileHash { get; init; }
    public required DocumentStatus Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public required string DisplayTitle { get; init; }
    public string? Description { get; init; }
    public string? Author { get; init; }
    public IEnumerable<string> Tags { get; init; } = [];
    public string? Notes { get; init; }
    public required bool IsOwner { get; init; }
}