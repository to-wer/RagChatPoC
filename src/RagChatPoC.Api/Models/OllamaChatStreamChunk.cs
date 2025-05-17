

using System.Text.Json.Serialization;

namespace RagChatPoC.Api.Models;

public class OllamaChatStreamChunk
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("message")]
    public MessageChunk? Message { get; set; }
    [JsonPropertyName("done")]
    public bool Done { get; set; }

    public class MessageChunk
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
