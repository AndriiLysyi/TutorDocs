using TutorDocs.Shared.Models.Dto;

namespace TutorDocs.Shared.Models.Responses;

public class PageResponse
{
    public Guid DocumentId { get; init; }
    public int  PageNumber { get; init; }
    public string PreSignUrl { get; init; } = "";
    public IReadOnlyList<ChunkOverviewDto> Chunks { get; init; } = [];
}