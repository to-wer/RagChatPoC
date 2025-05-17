using System.Text;
using System.Text.Json;
using RagChatPoC.Domain.Models;

namespace RagChatPoC.Web.ViewModels;

public class ChatViewModel
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ChatViewModel> _logger;
    public List<ChatMessage> Messages { get; set; } = new();
    public string CurrentMessage { get; set; } = string.Empty;
    public bool IsBusy { get; private set; }
    public string Model { get; set; } = "llama3.2";
    public bool UseStreaming { get; set; } = true;
    public bool IsStreaming { get; private set; }
    
    public event Action? OnNewToken;

    public ChatViewModel(IHttpClientFactory httpClientFactory,
        ILogger<ChatViewModel> logger)
    {
        _httpClient = httpClientFactory.CreateClient("RagChatPoC.Api");
        _logger = logger;
    }

    public async Task SendMessageAsync()
    {
        _logger.LogInformation("Sending message: {Message}", CurrentMessage);
        
        if (string.IsNullOrWhiteSpace(CurrentMessage)) return;

        var userMessage = new ChatMessage
        {
            Role = "user",
            Content = CurrentMessage
        };
        Messages.Add(userMessage);
        CurrentMessage = string.Empty;

        if (UseStreaming)
        {
            IsStreaming = true;
            IsBusy = true;

            _logger.LogDebug("Using streaming for response");
            var lastAssistant = new ChatMessage { Role = "assistant", Content = string.Empty };
            Messages.Add(lastAssistant);
            await foreach (var chunk in StreamChatResponseAsync())
            {
                if (IsBusy) IsBusy = false;
                AppendOrUpdateAssistantMessage(chunk);
            }
            IsStreaming = false;
            OnNewToken?.Invoke();
        }
        else
        {
            IsBusy = true;
            var response = await _httpClient.PostAsJsonAsync("v1/chat/completions", new ChatCompletionRequest
            {
                Model = Model,
                Stream = false,
                Messages = Messages
            });

            var data = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>();
            if (data?.Choices?.FirstOrDefault()?.Message is { } assistantMessage)
            {
                Messages.Add(assistantMessage);
            }
        IsBusy = false;
            
        }

    }

    private void AppendOrUpdateAssistantMessage(string chunk)
    {
        var lastAssistant = Messages.LastOrDefault(m => m.Role == "assistant");
        if (lastAssistant == null)
        {
            lastAssistant = new ChatMessage { Role = "assistant", Content = chunk };
            Messages.Add(lastAssistant);
        }
        else
        {
            lastAssistant.Content += chunk;
        }
        OnNewToken?.Invoke();
    }

    private async IAsyncEnumerable<string> StreamChatResponseAsync()
    {
        var request = new ChatCompletionRequest
        {
            Model = Model,
            Stream = true,
            Messages = Messages
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
        var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:")) continue;

            var jsonPart = line["data:".Length..].Trim();
            if (jsonPart == "[DONE]") yield break;

            string? content = null;
            try
            {
                var chunkJson = JsonSerializer.Deserialize<ChatCompletionStreamChunk>(jsonPart);
                content = chunkJson?.Choices?.FirstOrDefault()?.Delta?.Content;
            }
            catch (Exception)
            {
                // skip invalid chunks
            }

            if (!string.IsNullOrEmpty(content))
                yield return content;
        }
    }
}