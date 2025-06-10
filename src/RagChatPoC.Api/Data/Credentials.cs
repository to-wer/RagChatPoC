namespace RagChatPoC.Api.Data;

public class Credentials
{
    public Guid Id { get; set; }
    public required Dictionary<string, string> Data { get; set; } = new();
}