namespace RagChatPoC.Domain.Models;

public class OpenAiChatChoice
{
    public int Index { get; set; }
    public required OpenAiChatMessage Message { get; set; }
    public string FinishReason { get; set; } = "stop";
}