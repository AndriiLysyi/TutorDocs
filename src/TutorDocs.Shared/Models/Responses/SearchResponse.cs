using TutorDocs.Shared.Models.Dto;

namespace TutorDocs.Shared.Models.Responses;

public sealed record SearchResponse
{
    public IReadOnlyList<SearchResultDto> Results { get; init; } = [];
}