namespace RagChatPoC.Domain.Models;

/// <summary>
/// Represents a chat message exchanged between users, assistants, or the system.
/// </summary>
public class OpenAiChatMessage
{
    /// <summary>
    /// The role of the sender of the message. Possible values are "user", "assistant", or "system".
    /// </summary>
    public required string Role { get; set; }

    /// <summary>
    /// The content of the message.
    /// </summary>
    public string? Content { get; set; }
}