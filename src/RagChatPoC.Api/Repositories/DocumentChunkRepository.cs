using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using RagChatPoC.Api.Data;
using RagChatPoC.Api.Utils;

namespace RagChatPoC.Api.Repositories;

public class DocumentChunkRepository(RagDbContext context) : IDocumentChunkRepository
{
    public async Task SaveChunk(DocumentChunk doc)
    {
        await context.Chunks.AddAsync(doc);
        await context.SaveChangesAsync();
    }

    public Task<List<DocumentChunk>> GetRelevantChunks(string questionEmbedding)
    {
        var queryEmbedding = JsonSerializer.Deserialize<float[]>(questionEmbedding);

        var relevantChunks = context.Chunks
            .AsEnumerable() // Vorübergehend in Memory, besser wäre PgVector oder vektorisierte Suche
            .Select(c => new
            {
                Chunk = c,
                Similarity = EmbeddingUtils.CosineSimilarity(c.Embedding, queryEmbedding)
            })
            .OrderByDescending(x => x.Similarity)
            .Take(5) // oder 10
            .Select(x => x.Chunk)
            .ToList();
        return Task.FromResult(relevantChunks);
    }
}