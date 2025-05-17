using System.Text;
using RagChatPoC.Api.Data;

namespace RagChatPoC.Api.Utils;

public static class PromptBuilder
{
    /// <summary>
    /// Builds a prompt for the AI assistant based on a question and context.
    /// </summary>
    /// <param name="question">The user's question.</param>
    /// <param name="context">The context provided to the assistant.</param>
    /// <returns>A formatted prompt string.</returns>
    public static string Build(string question, string context)
    {
        var sb = new StringBuilder(256); // Pre-allocate capacity for better performance

        sb.AppendLine("You are an AI assistant helping the user based on uploaded documents.");
        sb.AppendLine();
        sb.AppendLine("Context:");
        sb.AppendLine(context);
        sb.AppendLine();
        sb.AppendLine($"User: {question}");
        sb.Append("Assistant:");

        return sb.ToString();
    }

    /// <summary>
    /// Builds a prompt for the AI assistant based on chat history, relevant documents, and a new question.
    /// </summary>
    /// <param name="history">The chat history between the user and the assistant.</param>
    /// <param name="chunks">Relevant document chunks for the context.</param>
    /// <param name="newQuestion">The user's new question.</param>
    /// <returns>A formatted prompt string.</returns>
    public static string Build(List<ChatMessage> history, List<DocumentChunk> chunks, string newQuestion)
    {
        var sb = new StringBuilder(512); // Pre-allocate capacity for better performance

        sb.AppendLine(
            "You are an AI assistant helping the user based on uploaded documents and previous chat context.");
        sb.AppendLine();
        sb.AppendLine("Chat history:");
        foreach (var message in history)
        {
            sb.AppendLine($"{(message.IsUser ? "User" : "Assistant")}: {message.Content}");
        }

        sb.AppendLine();
        sb.AppendLine("Relevant documents:");
        foreach (var chunk in chunks)
        {
            sb.AppendLine($"- {chunk.ChunkText}");
        }

        sb.AppendLine();
        sb.AppendLine($"User: {newQuestion}");
        sb.Append("Assistant:");

        return sb.ToString();
    }
}