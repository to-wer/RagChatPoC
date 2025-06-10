namespace RagChatPoC.Domain.Models.RagChat;

public class ExtendedChatCompletionResponse : OpenAiChatCompletionResponse
{
    public List<UsedContextChunk>? Context { get; set; }
}