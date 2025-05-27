using RagChatPoC.Domain.Models;

namespace RagChatPoC.Api.Models;

public class OllamaChatResponse
{
    public required string Model { get; set; }
    public DateTime CreatedAt { get; set; }

    public required ChatMessage Message { get; set; }
    public bool Done { get; set; }
    public long TotalDuration { get; set; }
    public long LoadDuration { get; set; }
    public int PromptEvalCount { get; set; }
    public long PromptEvalDuration { get; set; }
    public int EvalCount { get; set; }
    public long EvalDuration { get; set; }
}