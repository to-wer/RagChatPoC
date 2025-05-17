using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RagChatPoC.Api.Services.Interfaces;
using RagChatPoC.Api.Utils;
using RagChatPoC.Api.Models;

namespace RagChatPoC.Api.Services;

public class ChatService(IConfiguration configuration, HttpClient httpClient) : IChatService
{
    public async Task<string?> GetAnswerAsync(string question, string context)
    {
        var prompt = PromptBuilder.Build(question, context);
        
        return await GetAnswerAsync(prompt);
    }

    public async Task<string?> GetAnswerAsync(string prompt)
    {
        if (configuration["OpenAi:ApiKey"] is { Length: > 0 })
        {
            return await GetOpenAiAnswerAsync(prompt);
        }

        return await GetOllamaAnswerAsync(prompt);;
    }

    public IAsyncEnumerable<string> GetStreamingCompletionAsync(string prompt)
    {
        throw new NotImplementedException();
    }

    private async Task<string?> GetOpenAiAnswerAsync(string prompt)
    {
        var payload = new
        {
            model = "gpt-4", // oder "gpt-3.5-turbo"
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", configuration["OpenAi:ApiKey"]);
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var res = await httpClient.SendAsync(req);
        var json = await res.Content.ReadAsStringAsync();

        var parsed = JsonDocument.Parse(json);
        return parsed.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
    }

    private async Task<string?> GetOllamaAnswerAsync(string prompt)
    {
        var payload = new
        {
            model = "llama3.2", // oder z. B. "mistral"
            prompt,
            stream = false
        };

        var res = await httpClient.PostAsync("http://localhost:11434/api/generate",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

        var content = await res.Content.ReadAsStringAsync();
        var parsed = JsonDocument.Parse(content);

        return parsed.RootElement.GetProperty("response").GetString();
    }
}