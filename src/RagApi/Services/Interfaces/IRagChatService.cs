using RagApi.Models;

namespace RagApi.Services.Interfaces;

public interface IRagChatService
{
    Task<ChatCompletionResponse> GetCompletionAsync(ChatCompletionRequest request);
    IAsyncEnumerable<string> GetStreamingCompletionAsync(ChatCompletionRequest request);
}