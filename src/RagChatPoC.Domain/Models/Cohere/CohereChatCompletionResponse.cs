namespace RagChatPoC.Domain.Models.Cohere;

public class CohereChatCompletionResponse
{
    public string Id { get; set; } = null!;
    public string FinishReason { get; set; } = null!;
    public CohereMessage Message { get; set; } = null!;
    public CohereUsage Usage { get; set; } = null!;
}