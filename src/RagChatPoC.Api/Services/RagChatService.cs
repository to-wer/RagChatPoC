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
    ILogger<RagChatService> logger,
    IChatHelperService chatHelperService) : IRagChatService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("OllamaClient");

    public async Task<ChatCompletionResponse> GetCompletion(ChatCompletionRequest request)
    {
        var latestUserMessage = chatHelperService.GetLatestUserMessage(request);
        if (latestUserMessage == null)
            return new ChatCompletionResponse();

        var relevantChunks = await chatHelperService.GetRelevantChunks(latestUserMessage);

        request = await chatHelperService.PrepareChatRequest(request, relevantChunks);

        var response = await _httpClient.PostAsync("api/chat",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();

        var ollamaChatResponse = await response.Content.ReadFromJsonAsync<OllamaChatResponse>();

        return new ChatCompletionResponse
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
            Model = ollamaChatResponse?.Model ?? string.Empty,
            Context =
                relevantChunks.Select(x => new UsedContextChunk()
                {
                    Snippet = x.Snippet.Length > 300 ? x.Snippet[..300] + "..." : x.Snippet,
                    SourceFile = x.SourceFile,
                    Score = x.Score
                }).ToList()
        };
    }

    public async IAsyncEnumerable<string> GetStreamingCompletion(ChatCompletionRequest request)
    {
        var latestUserMessage = chatHelperService.GetLatestUserMessage(request);
        if (latestUserMessage == null)
            yield break;

        var relevantChunks = await chatHelperService.GetRelevantChunks(latestUserMessage);

        var contextJson = JsonSerializer.Serialize(new
        {
            context = relevantChunks.Select(x => new UsedContextChunk()
            {
                Snippet = x.Snippet.Length > 300 ? x.Snippet[..300] + "..." : x.Snippet,
                SourceFile = x.SourceFile,
                Score = x.Score
            })
        });
        logger.LogInformation(contextJson);
        yield return contextJson; // << Erstes data:-Event enthält Kontext

        request = await chatHelperService.PrepareChatRequest(request, relevantChunks);

        var requestJson = JsonSerializer.Serialize(request);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/api/chat")
        {
            Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
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
                var chunk = JsonSerializer.Deserialize<OllamaChatStreamChunk>(line);
                content = chunk?.Message?.Content;
            }
            catch
            {
                // Skip invalid lines
            }

            logger.LogInformation(content);
            if (!string.IsNullOrEmpty(content))
                yield return JsonSerializer.Serialize(new ChatCompletionStreamChunk()
                {
                    Id = "chatcmpl-" + Guid.NewGuid().ToString("N"),
                    Object = "chat.completion.chunk",
                    Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    Model = $"ollama-{request.Model}",
                    Choices = new[]
                    {
                        new ChatCompletionStreamChunk.StreamChoice()
                        {
                            Delta = new ChatMessage() { Content = content},
                            Index = 0,
                            FinishReason = (string?)null
                        }
                    }.ToList()
                });
        }
    }
}