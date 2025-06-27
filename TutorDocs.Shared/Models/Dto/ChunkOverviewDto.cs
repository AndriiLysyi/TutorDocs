namespace TutorDocs.Shared.Models.Dto;

public record ChunkOverviewDto
(
    Guid ChunkId,
    int  TokenStart,
    int  TokenEnd
);