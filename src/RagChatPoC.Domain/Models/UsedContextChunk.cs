namespace RagChatPoC.Domain.Models;

public class UsedContextChunk
{
    public required string SourceFile { get; set; }
    public required string Snippet { get; set; }
    public float? Score { get; set; }
}