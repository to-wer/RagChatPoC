namespace RagChatPoC.Api.Data;

public class ChatModel
{
    public Guid Id { get; set; }
    public required string DisplayName { get; set; }
    public required string ModelName { get; set; }
    public required string Provider { get; set; }
    public Guid CredentialsId { get; set; }

    public virtual Credentials Credentials { get; set; }
}