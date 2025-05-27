namespace RagChatPoC.Domain.Models;

public class OpenAiChatCompletionResponse
{
    public string Id { get; set; } = $"chatcmpl-{Guid.NewGuid()}";
    public string Object { get; set; } = "chat.completion";
    public long Created { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public string? Model { get; set; }
    public List<OpenAiChatChoice> Choices { get; set; } = [];
    public OpenAiUsage? Usage { get; set; }
}