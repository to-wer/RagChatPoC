using RagChatPoC.Domain.Models;
using RagChatPoC.Domain.Models.RagChat;

namespace RagChatPoC.Api.Services.Interfaces;

public interface IRagChatService
{
    Task<ExtendedChatCompletionResponse> GetCompletion(ExtendedChatCompletionRequest request);
    IAsyncEnumerable<string> GetStreamingCompletion(ExtendedChatCompletionRequest request);
}