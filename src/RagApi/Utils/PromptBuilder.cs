using System.Text;
using RagApi.Data;

namespace RagApi.Utils;

public static class PromptBuilder
{
    public static string Build(string question, string context)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("You are an AI assistant helping the user based on uploaded documents.");
        sb.AppendLine();
        
        sb.AppendLine();
        sb.AppendLine("Context:");
        sb.AppendLine(context);
        
        sb.AppendLine();
        sb.AppendLine($"User: {question}");
        sb.Append("Assistant:");
        
        return sb.ToString();
    }
    
    public static string Build(List<ChatMessage> history, List<DocumentChunk> chunks, string newQuestion)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are an AI assistant helping the user based on uploaded documents and previous chat context.");
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
