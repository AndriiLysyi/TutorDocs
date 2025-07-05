using TutorDocs.Shared.Models.Enums;

namespace TutorDocs.Shared.Models.Dto;

public class DocumentDto
{
    public required Guid Id { get; set; }
    public required string OriginalFilename { get; set; }
    public required string FileHash { get; set; }
    public required DocumentStatus Status { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}