using System.Text;
using System.Text.Json;
using RagChatPoC.Api.Models;
using RagChatPoC.Api.Repositories;
using RagChatPoC.Api.Services.Interfaces;
using RagChatPoC.Domain.Models;

namespace RagChatPoC.Api.Services;

public class RagChatService(
    IDocumentChunkRepository documentChunkRepository,
    IEmbeddingService embeddingService,
    IHttpClientFactory httpClientFactory,
    ILogger<RagChatService> logger) : IRagChatService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("OllamaClient");
    
    public async Task<ChatCompletionResponse> GetCompletionAsync(ChatCompletionRequest request)
    {
        var ollamaRequest = await CreateOllamaChatRequest(request);

        if (ollamaRequest == null)
            return new ChatCompletionResponse();

        var response = await _httpClient.PostAsync("api/chat",
            new StringContent(JsonSerializer.Serialize(ollamaRequest), Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();

        var ollamaChatResponse = await response.Content.ReadFromJsonAsync<OllamaChatResponse>();

        return new ChatCompletionResponse()
        {
            Choices =
            [
                new ChatChoice
                {
                    Index = 0,
                    Message = new ChatMessage
                    {
                        Role = "assistant",
                        Content = ollamaChatResponse?.Message.Content ?? string.Empty
                    }
                }
            ],
            Model = ollamaChatResponse?.Model ?? string.Empty
        };
    }

    public async IAsyncEnumerable<ChatCompletionStreamChunk> GetStreamingCompletionAsync(ChatCompletionRequest request)
    {
        var ollamaRequest = await CreateOllamaChatRequest(request);
        if (ollamaRequest == null)
            yield break;

        var requestJson = JsonSerializer.Serialize(ollamaRequest);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/chat");
        httpRequest.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            //string? content = null;
            OllamaChatStreamChunk? chunk = null;
            try
            {
                chunk = JsonSerializer.Deserialize<OllamaChatStreamChunk>(line);
                //content = json?.Message?.Content;
            }
            catch (JsonException ex)
            {
                logger.LogWarning("Failed to deserialize JSON: {Message}", ex.Message);
                // Ignore malformed chunks
            }

            if (chunk != null)
            {
                yield return new ChatCompletionStreamChunk()
                {
                    Choices = new()
                    {
                        new ChatCompletionStreamChunk.StreamChoice()
                        {
                            Delta = new()
                            {
                                Content = chunk.Message?.Content
                            }
                        }
                    }
                };
            }
        }
    }

    private async Task<OllamaChatRequest?> CreateOllamaChatRequest(ChatCompletionRequest request)
    {
        var lastUserMessage = request.Messages
            .LastOrDefault(m => m.Role == "user");

        if (lastUserMessage == null)
            return null;

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
            new() { Role = "system", Content = systemPrompt }
        };
        newMessages.AddRange(request.Messages);

        return new OllamaChatRequest
        {
            Model = request.Model,
            Stream = request.Stream,
            Messages = newMessages.ToArray()
        };
    }
}