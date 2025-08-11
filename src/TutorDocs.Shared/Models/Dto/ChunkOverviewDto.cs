namespace TutorDocs.Shared.Models.Dto;

public sealed record ChunkOverviewDto
(
    Guid ChunkId,
    int  TokenStart,
    int  TokenEnd
);