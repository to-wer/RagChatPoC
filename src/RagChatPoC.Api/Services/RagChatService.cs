using System.Text;
using System.Text.Json;
using RagChatPoC.Api.Models;
using RagChatPoC.Api.Services.Interfaces;
using RagChatPoC.Domain.Models;

namespace RagChatPoC.Api.Services;

public class RagChatService(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    IChatHelperService chatHelperService) : IRagChatService
{
    public async Task<ExtendedChatCompletionResponse> GetCompletion(ExtendedChatCompletionRequest request)
    {
        var latestUserMessage = chatHelperService.GetLatestUserMessage(request);
        if (latestUserMessage == null)
            return new ExtendedChatCompletionResponse();

        var relevantChunks = await chatHelperService.GetRelevantChunks(latestUserMessage);

        request = await chatHelperService.PrepareChatRequest(request, relevantChunks);

        HttpClient httpClient;
        string url;
        switch (request.Provider?.ToLower())
        {
            case "ollama":
                httpClient = httpClientFactory.CreateClient("OllamaClient");
                url = "api/chat";
                break;
            case "cohere":
                httpClient = httpClientFactory.CreateClient("CohereClient");
                url = "v2/chat";
                break;
            default:
                throw new ArgumentException($"Unsupported provider: {request.Provider}", nameof(request.Provider))
                {
                    HelpLink = null,
                    HResult = 0,
                    Source = null
                };
        }

        var response = await httpClient.PostAsync(url,
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();

        var ollamaChatResponse = await response.Content.ReadFromJsonAsync<OllamaChatResponse>();

        return new ExtendedChatCompletionResponse()
        {
            Choices =
            [
                new OpenAiChatChoice
                {
                    Index = 0,
                    Message = new OpenAiChatMessage
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

    public async IAsyncEnumerable<string> GetStreamingCompletion(ExtendedChatCompletionRequest request)
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
        yield return contextJson; // << Erstes data:-Event enthält Kontext

        request = await chatHelperService.PrepareChatRequest(request, relevantChunks);

        var requestJson = JsonSerializer.Serialize(request);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{configuration["OLLAMA_HOST"]}/api/chat")
        {
            Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
        };

        var httpClient = httpClientFactory.CreateClient("OllamaClient");
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
                var chunk = JsonSerializer.Deserialize<OllamaChatStreamChunk>(line);
                content = chunk?.Message?.Content;
            }
            catch
            {
                // Skip invalid lines
            }

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
                            Delta = new OpenAiChatMessage
                            {
                                Content = content,
                                Role = "assistant"
                            },
                            Index = 0,
                            FinishReason = (string?)null
                        }
                    }.ToList()
                });
        }
    }
}