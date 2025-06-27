namespace TutorDocs.Shared.Models.Entities;

public class DocumentChunk
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int TokenStart { get; set; }
    public int TokenEnd { get; set; }
    public PageInfo PageInfo { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    
    public Document Document { get; set; } = null!; 
}