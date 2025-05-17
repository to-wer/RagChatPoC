namespace RagChatPoC.Api.Models;

public class ChatCompletionResponse
{
    public string Id { get; set; } = $"chatcmpl-{Guid.NewGuid()}";
    public string Object { get; set; } = "chat.completion";
    public long Created { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public string Model { get; set; } = "local-rag";
    public List<ChatChoice> Choices { get; set; } = new();
}