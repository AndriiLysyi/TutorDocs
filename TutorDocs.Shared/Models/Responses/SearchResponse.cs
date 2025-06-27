using TutorDocs.Shared.Models.Dto;

namespace TutorDocs.Shared.Models.Responses;

public class SearchResponse
{
    public IReadOnlyList<SearchResultDto> Results { get; init; } = [];
}