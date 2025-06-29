using System.Text.Json.Serialization;

namespace TutorDocs.Shared.Models.Entities;

public class DocumentMetadata
{
    [JsonPropertyName("display_title")]
    public string? DisplayTitle { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("author")]
    public string? Author { get; set; }
    
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];
    
    [JsonPropertyName("notes")]
    public string? Notes { get; set; } 
}