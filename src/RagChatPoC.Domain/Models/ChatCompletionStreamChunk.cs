using System.Text.Json.Serialization;

namespace RagChatPoC.Domain.Models;

public class ChatCompletionStreamChunk
{
    [JsonPropertyName("choices")] public List<StreamChoice>? Choices { get; set; }

    public class StreamChoice
    {
        [JsonPropertyName("delta")] public ChatMessage? Delta { get; set; }
    }
}