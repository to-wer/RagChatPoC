using System.Text;
using System.Text.Json;
using RagChatPoC.Api.Services.Interfaces;

namespace RagChatPoC.Api.Services;

public class EmbeddingService(IConfiguration configuration, HttpClient httpClient) : IEmbeddingService
{
    
    public async Task<string> GetEmbeddingAsync(string text)
    {
        if (configuration["OpenAi:ApiKey"] is { Length: > 0 })
        {
            return await GetOpenAiEmbeddingAsync(text);
        }

        return await GetOllamaEmbeddingAsync(text);
    }

    private Task<string> GetOpenAiEmbeddingAsync(string text)
    {
        throw new NotImplementedException();
    }

    private async Task<string> GetOllamaEmbeddingAsync(string input)
    {
        var requestBody = new { model = "nomic-embed-text", prompt = input };
        var response = await httpClient.PostAsync("http://localhost:11434/api/embeddings",
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));

        var responseString = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(responseString);
        return json.RootElement.GetProperty("embedding").ToString();
    }
}