namespace TutorDocs.Shared.Models.Requests;

public sealed record SearchRequest
{
    public string Query  { get; init; } = "";
}