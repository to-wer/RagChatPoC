namespace RagApi.Models;

public class OllamaChatStreamChunk
{
    public string? Model { get; set; }
    public DateTime CreatedAt { get; set; }
    public MessageChunk? Message { get; set; }
    public bool Done { get; set; }

    public class MessageChunk
    {
        public string? Role { get; set; }
        public string? Content { get; set; }
    }
}
