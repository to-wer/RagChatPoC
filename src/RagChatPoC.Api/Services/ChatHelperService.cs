using RagChatPoC.Api.Data;
using RagChatPoC.Api.Repositories;
using RagChatPoC.Api.Services.Interfaces;
using RagChatPoC.Domain.Models;
using ChatMessage = RagChatPoC.Domain.Models.ChatMessage;

namespace RagChatPoC.Api.Services;

public class ChatHelperService(IEmbeddingService embeddingService,
    IDocumentChunkRepository documentChunkRepository) : IChatHelperService
{
    public Task<ChatCompletionRequest> PrepareChatRequest(ChatCompletionRequest request, List<UsedContextChunk> relevantChunks)
    {
        var context = string.Join("\n---\n", relevantChunks.Select(c => c.Snippet));

        var systemPrompt = $"""
                                You are a helpful assistant. Use the following context to answer the user's question.

                                Context:
                                {context}
                            """;
        
        var newMessages = new List<ChatMessage>
        {
            new() { Role = "system", Content = systemPrompt }
        };
        newMessages.AddRange(request.Messages);
        
        return Task.FromResult(new ChatCompletionRequest()
        {
            Messages = newMessages,
            Model = request.Model,
            Stream = request.Stream
        });
    }

    public ChatMessage? GetLatestUserMessage(ChatCompletionRequest request)
    {
        return request.Messages
            .LastOrDefault(m => m.Role == "user");
    }

    public async Task<List<UsedContextChunk>> GetRelevantChunks(ChatMessage chatMessage)
    {
        var questionEmbedding = await embeddingService.GetEmbeddingAsync(chatMessage.Content);
        var relevantChunks = await documentChunkRepository.GetRelevantChunks(questionEmbedding);
        return relevantChunks;
    }
}