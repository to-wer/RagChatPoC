using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Pgvector;

namespace RagApi.Data;

public class DocumentChunk
{
    public int Id { get; set; }
    public string SourceFile { get; set; } = null!;
    public string ChunkText { get; set; } = null!;
    public string EmbeddingJson { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [NotMapped]
    public float[] Embedding => JsonSerializer.Deserialize<float[]>(EmbeddingJson)!;
}