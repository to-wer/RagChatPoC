namespace RagChatPoC.Domain.Models.Credentials;

public class CreateCredentialsDto
{
    public required Dictionary<string, string> Data { get; set; } = new();
}