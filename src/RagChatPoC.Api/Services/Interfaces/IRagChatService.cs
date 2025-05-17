using RagChatPoC.Domain.Models;

namespace RagChatPoC.Api.Services.Interfaces;

public interface IRagChatService
{
    Task<ChatCompletionResponse> GetCompletionAsync(ChatCompletionRequest request);
    IAsyncEnumerable<ChatCompletionStreamChunk> GetStreamingCompletionAsync(ChatCompletionRequest request);
}