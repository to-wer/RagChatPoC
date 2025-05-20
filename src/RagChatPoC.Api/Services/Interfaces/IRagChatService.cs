using RagChatPoC.Domain.Models;

namespace RagChatPoC.Api.Services.Interfaces;

public interface IRagChatService
{
    Task<ChatCompletionResponse> GetCompletion(ChatCompletionRequest request);
    IAsyncEnumerable<ChatCompletionStreamChunk> GetStreamingCompletion(ChatCompletionRequest request);
}