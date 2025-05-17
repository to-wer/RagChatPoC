namespace RagChatPoC.Api.Models;

public class ChatMessage
{
    public string Role { get; set; } = null!; // user | assistant | system
    public string Content { get; set; } = null!;
}