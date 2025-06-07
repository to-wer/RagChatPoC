namespace RagChatPoC.Domain.Models.Cohere;

public class CohereMessage
{
    public string Role { get; set; } = null!;
    public List<CohereMessageContent> Content { get; set; } = new();
}