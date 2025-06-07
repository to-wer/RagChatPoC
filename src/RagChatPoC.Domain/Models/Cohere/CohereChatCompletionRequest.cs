namespace RagChatPoC.Domain.Models.Cohere;

public class CohereChatCompletionRequest
{
    public string Model { get; set; } = "command-r";
    public List<OpenAiChatMessage> Messages { get; set; } = new();

}