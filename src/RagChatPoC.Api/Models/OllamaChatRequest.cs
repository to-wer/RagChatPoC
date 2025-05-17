namespace RagChatPoC.Api.Models;

public class OllamaChatRequest
{
    public string Model { get; set; } = "llama3.2";
    public bool Stream { get; set; }
    public ChatMessage[] Messages { get; set; } = [];
}