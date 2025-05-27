namespace RagChatPoC.Domain.Models;

public class ExtendedChatCompletionResponse : OpenAiChatCompletionResponse
{
    public List<UsedContextChunk>? Context { get; set; }
}