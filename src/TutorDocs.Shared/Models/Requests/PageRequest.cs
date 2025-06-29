namespace TutorDocs.Shared.Models.Requests;

public class PageRequest
{
    public Guid DocumentId { get; init; }

    public int  PageNumber { get; init; } 
}