namespace RagChatPoC.Domain.Models.RagChat;

public class ExtendedChatCompletionRequest : OpenAiChatCompletionRequest
{
    public string Provider { get; set; } = "ollama";
}