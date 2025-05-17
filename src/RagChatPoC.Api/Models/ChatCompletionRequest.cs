namespace RagChatPoC.Api.Models;

public class ChatCompletionRequest
{
    public string Model { get; set; } = "llama3.2";

    public List<ChatMessage> Messages { get; set; } = new();
    public bool Stream { get; set; } = true;
}