using RagChatPoC.Api.Data;
using RagChatPoC.Domain.Models;
using RagChatPoC.Domain.Models.RagChat;

namespace RagChatPoC.Api.Services.Interfaces;

public interface IChatHelperService
{
    Task<ExtendedChatCompletionRequest> PrepareChatRequest(ExtendedChatCompletionRequest request, List<UsedContextChunk> relevantChunks);
    OpenAiChatMessage? GetLatestUserMessage(OpenAiChatCompletionRequest request);
    Task<List<UsedContextChunk>> GetRelevantChunks(OpenAiChatMessage openAiChatMessage);
}