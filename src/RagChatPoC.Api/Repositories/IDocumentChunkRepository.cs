using RagChatPoC.Api.Data;

namespace RagChatPoC.Api.Repositories;

public interface IDocumentChunkRepository
{
    Task SaveChunk(DocumentChunk doc);
    Task<List<DocumentChunk>> GetRelevantChunks(string questionEmbedding);
}