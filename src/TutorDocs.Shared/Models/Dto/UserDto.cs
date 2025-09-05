namespace TutorDocs.Shared.Models.Dto;

public sealed record UserDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required DateTime CreatedAt { get; init; }
}