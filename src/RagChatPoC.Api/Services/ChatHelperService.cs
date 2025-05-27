using RagChatPoC.Api.Data;
using RagChatPoC.Api.Repositories;
using RagChatPoC.Api.Services.Interfaces;
using RagChatPoC.Domain.Models;

namespace RagChatPoC.Api.Services;

public class ChatHelperService(IEmbeddingService embeddingService,
    IDocumentChunkRepository documentChunkRepository) : IChatHelperService
{
    public Task<ExtendedChatCompletionRequest> PrepareChatRequest(ExtendedChatCompletionRequest request, List<UsedContextChunk> relevantChunks)
    {
        var context = string.Join("\n---\n", relevantChunks.Select(c => c.Snippet));

        var systemPrompt = $"""
                                You are a helpful assistant. Use the following context to answer the user's question.

                                Context:
                                {context}
                            """;
        
        var newMessages = new List<OpenAiChatMessage>
        {
            new() { Role = "system", Content = systemPrompt }
        };
        newMessages.AddRange(request.Messages);
        
        request.Messages = newMessages;
        return Task.FromResult(request);
    }

    public OpenAiChatMessage? GetLatestUserMessage(OpenAiChatCompletionRequest request)
    {
        return request.Messages
            .LastOrDefault(m => m.Role == "user");
    }

    public async Task<List<UsedContextChunk>> GetRelevantChunks(OpenAiChatMessage openAiChatMessage)
    {
        var questionEmbedding = await embeddingService.GetEmbeddingAsync(openAiChatMessage.Content);
        var relevantChunks = await documentChunkRepository.GetRelevantChunks(questionEmbedding);
        return relevantChunks;
    }
}