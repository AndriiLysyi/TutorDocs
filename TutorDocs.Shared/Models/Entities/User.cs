namespace TutorDocs.Shared.Models.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public ICollection<DocumentOwner> OwnedDocuments { get; set; } = [];
    public ICollection<AccessControlList> SharedDocuments { get; set; } = [];
}