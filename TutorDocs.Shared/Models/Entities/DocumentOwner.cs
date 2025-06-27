namespace TutorDocs.Shared.Models.Entities;

public class DocumentOwner
{
    public Guid DocumentId { get; set; }
    public Guid UserId { get; set; }
    public DocumentMetadata Metadata { get; set; } = new();
    public DateTime AddedAt { get; set; }
    
    public Document Document { get; set; } = null!;
    public User User { get; set; } = null!;
}