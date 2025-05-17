namespace RagChatPoC.Api.Data;

public class ChatSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Title { get; set; } // optional: erster Prompt oder Nutzerdefiniert
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}