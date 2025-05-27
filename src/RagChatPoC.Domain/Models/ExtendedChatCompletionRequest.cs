namespace RagChatPoC.Domain.Models;

public class ExtendedChatCompletionRequest : OpenAiChatCompletionRequest
{
    public string Provider { get; set; } = "ollama";
}