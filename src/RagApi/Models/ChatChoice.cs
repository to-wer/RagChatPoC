namespace RagApi.Models;

public class ChatChoice
{
    public int Index { get; set; }
    public ChatMessage Message { get; set; } = null!;
    public string FinishReason { get; set; } = "stop";
}