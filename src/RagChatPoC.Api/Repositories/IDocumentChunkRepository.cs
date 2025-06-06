using RagChatPoC.Api.Data;
using RagChatPoC.Domain.Models;

namespace RagChatPoC.Api.Repositories;

public interface IDocumentChunkRepository
{
    Task<IEnumerable<DocumentDto>> GetAllDocuments();
    Task SaveChunk(DocumentChunk doc);
    Task<List<UsedContextChunk>> GetRelevantChunks(string questionEmbedding);
    Task DeleteBySource(string sourceFile);
}