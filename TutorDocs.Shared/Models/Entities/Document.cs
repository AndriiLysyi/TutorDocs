using TutorDocs.Shared.Models.Enums;

namespace TutorDocs.Shared.Models.Entities;

public class Document
{
    public Guid Id { get; set; }
    public string OriginalFilename { get; set; } = string.Empty;
    public string FileHash { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public ICollection<DocumentOwner> Owners { get; set; } = [];
    public ICollection<AccessControlList> SharedWith { get; set; } = [];
    public ICollection<DocumentChunk> Chunks { get; set; } = [];
}