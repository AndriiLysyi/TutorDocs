namespace TutorDocs.Shared.Models.Dto;

public sealed record SearchResultDto
(
    Guid  DocumentId,
    Guid  ChunkId,
    int   PageNumber,
    string DocumentName
);