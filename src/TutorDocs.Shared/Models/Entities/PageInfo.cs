using System.Text.Json.Serialization;

namespace TutorDocs.Shared.Models.Entities;

public class PageInfo
{
    [JsonPropertyName("page_number")]
    public int PageNumber { get; set; }
    
    [JsonPropertyName("s3_path")]
    public string S3Path { get; set; } = string.Empty;
}