namespace TutorDocs.Shared.Models.Entities;

public class AccessControlList
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Guid UserId { get; set; }
    public Guid SharedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Document Document { get; set; } = null!;
    public User User { get; set; } = null!;
}