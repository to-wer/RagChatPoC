using System.Text.Json.Serialization;

namespace RagChatPoC.Domain.Models;

public class ChatCompletionStreamChunk
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("object")] public string? Object { get; set; }
    [JsonPropertyName("created")] public long? Created { get; set; }
    [JsonPropertyName("model")] public string? Model { get; set; }
    [JsonPropertyName("choices")] public List<StreamChoice>? Choices { get; set; }

    public class StreamChoice
    {
        [JsonPropertyName("index")] public int? Index { get; set; }
        [JsonPropertyName("delta")] public ChatMessage? Delta { get; set; }
        [JsonPropertyName("finish_reason")] public string? FinishReason { get; set; }
    }
}