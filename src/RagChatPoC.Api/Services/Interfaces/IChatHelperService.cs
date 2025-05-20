using RagChatPoC.Api.Data;
using RagChatPoC.Domain.Models;
using ChatMessage = RagChatPoC.Domain.Models.ChatMessage;

namespace RagChatPoC.Api.Services.Interfaces;

public interface IChatHelperService
{
    Task<ChatCompletionRequest> PrepareChatRequest(ChatCompletionRequest request, List<UsedContextChunk> relevantChunks);
    ChatMessage? GetLatestUserMessage(ChatCompletionRequest request);
    Task<List<UsedContextChunk>> GetRelevantChunks(ChatMessage chatMessage);
}