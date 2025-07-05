namespace TutorDocs.Shared.Models.Dto;

public class UserDto
{
    public required Guid Id { get; set; }
    public required string Email { get; set; }
    public required DateTime CreatedAt { get; set; }
}