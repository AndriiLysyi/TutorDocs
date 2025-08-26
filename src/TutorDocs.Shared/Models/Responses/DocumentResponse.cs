using TutorDocs.Shared.Models.Enums;

namespace TutorDocs.Shared.Models.Responses;
public sealed record DocumentResponse
{
    public required Guid Id { get; init; }
    
    public required string OriginalFilename { get; init; }
    
    public required DocumentStatus Status { get; init; }
    
    public required DateTime CreatedAt { get; init; }
    
    public DateTime? UpdatedAt { get; init; }
}