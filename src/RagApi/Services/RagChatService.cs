using System.Text;
using System.Text.Json;
using RagApi.Models;
using RagApi.Repositories;
using RagApi.Services.Interfaces;
using RagApi.Utils;

namespace RagApi.Services;

public class RagChatService(
    IDocumentChunkRepository documentChunkRepository,
    IChatService chatService,
    IEmbeddingService embeddingService,
    HttpClient httpClient,
    ILogger<RagChatService> logger) : IRagChatService
{
    public async Task<ChatCompletionResponse> GetCompletionAsync(ChatCompletionRequest request)
    {
        var lastUserMessage = request.Messages
            .LastOrDefault(m => m.Role == "user");

        if (lastUserMessage == null)
            throw new ArgumentException("No user message found.");

        var questionEmbedding = await embeddingService.GetEmbeddingAsync(lastUserMessage.Content);
        
        var relevantChunks = await documentChunkRepository.GetRelevantChunks(questionEmbedding);

        var contextText = string.Join("\n---\n", relevantChunks.Select(c => c.ChunkText));

        // Prompt bauen
        var fullPrompt = new StringBuilder();
        fullPrompt.AppendLine("Beziehe dich auf den folgenden Kontext, um die Frage zu beantworten:");
        fullPrompt.AppendLine(contextText);
        fullPrompt.AppendLine("\nFrage:");
        fullPrompt.AppendLine(lastUserMessage.Content);

        var response = await chatService.GetAnswerAsync(fullPrompt.ToString());

        return new ChatCompletionResponse
        {
            Choices = new List<ChatChoice>
            {
                new ChatChoice
                {
                    Index = 0,
                    Message = new ChatMessage
                    {
                        Role = "assistant",
                        Content = response ?? string.Empty
                    }
                }
            }
        };
    }

    public async IAsyncEnumerable<string> GetStreamingCompletionAsync(ChatCompletionRequest request)
    {
        var lastUserMessage = request.Messages
            .LastOrDefault(m => m.Role == "user");

        if (lastUserMessage == null)
            yield break;
        
        var questionEmbedding = await embeddingService.GetEmbeddingAsync(lastUserMessage.Content);
        var relevantChunks = await documentChunkRepository.GetRelevantChunks(questionEmbedding);
        
        var context = string.Join("\n---\n", relevantChunks.Select(c => c.ChunkText));

        var systemPrompt = $"""
                                You are a helpful assistant. Use the following context to answer the user's question.

                                Context:
                                {context}
                            """;

        // Neue Nachrichtenliste mit Kontext als System-Nachricht
        var newMessages = new List<ChatMessage>
        {
            new ChatMessage { Role = "system", Content = systemPrompt }
        };
        newMessages.AddRange(request.Messages);
        
        var ollamaRequest = new
        {
            model = request.Model ?? "llama3.2",
            stream = true,
            messages = newMessages.Select(m => new
            {
                role = m.Role.ToLowerInvariant(),
                content = m.Content
            }).ToArray()
        };

        var requestJson = JsonSerializer.Serialize(ollamaRequest);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/api/chat")
        {
            Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
        };

        using var response = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string? content = null;
            try
            {
                var json = JsonSerializer.Deserialize<OllamaChatStreamChunk>(line);
                content = json?.Message?.Content;
            }
            catch (JsonException ex)
            {
                logger.LogWarning("Failed to deserialize JSON: {Message}", ex.Message);
                // Ignore malformed chunks
            }

            if (!string.IsNullOrEmpty(content))
            {
                yield return content;
            }
        }
    }

}