using Pgvector;
using RagChatPoC.Api.Data;
using RagChatPoC.Api.Repositories;
using RagChatPoC.Api.Services.Interfaces;

namespace RagChatPoC.Api.Services;

public class IndexService(IEmbeddingService embeddingService,
    IDocumentChunkRepository documentChunkRepository) : IIndexService
{
    public Task SaveChunkAsync(string sourceFileName, object chunk, object embedding)
    {
        throw new NotImplementedException();
    }

    public async Task IndexChunksAsync(IEnumerable<TextChunk> chunks)
    {
        foreach (var chunk in chunks.Where(c => !string.IsNullOrWhiteSpace(c.Content)))
        {
            var embedding = await embeddingService.GetEmbeddingAsync(chunk.Content);
            
            var doc = new DocumentChunk()
            {
                SourceFile = chunk.Source,
                ChunkText = chunk.Content,
                EmbeddingJson = embedding
            };

            await documentChunkRepository.SaveChunk(doc);
        }
    }
}
