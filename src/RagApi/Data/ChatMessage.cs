namespace RagApi.Data;

public class ChatMessage
{
    public int Id { get; set; }
    public Guid SessionId { get; set; }
    public ChatSession Session { get; set; } = null!;
    public bool IsUser { get; set; } // true: user, false: assistant
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}