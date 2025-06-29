namespace TutorDocs.Shared.Models.Entities;

public class QdrantVector
{
    public Guid ChunkId { get; set; }
    public Guid DocumentId { get; set; }
    public float[] EmbeddingVector { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    
    public DocumentChunk DocumentChunk { get; set; } = null!;
}