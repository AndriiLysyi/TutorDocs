namespace TutorDocs.Shared.Models.Dto;

public record SearchResultDto
(
    Guid  DocumentId,
    Guid  ChunkId,
    int   PageNumber,
    string DocumentName
);