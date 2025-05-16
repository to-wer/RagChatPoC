using RagApi.Data;

namespace RagApi.Repositories;

public interface IDocumentChunkRepository
{
    Task SaveChunk(DocumentChunk doc);
    Task<List<DocumentChunk>> GetRelevantChunks(string questionEmbedding);
}