using RagChatPoC.Api.Models;

namespace RagChatPoC.Api.Services.Interfaces;

public interface IIndexService
{
    Task IndexChunksAsync(IEnumerable<TextChunk> chunks);
}