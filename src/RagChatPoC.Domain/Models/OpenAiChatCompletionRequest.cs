namespace RagChatPoC.Domain.Models;

public class OpenAiChatCompletionRequest
{
    public string Model { get; set; } = "llama3.2";
    public List<OpenAiChatMessage> Messages { get; set; } = [];
    public bool Stream { get; set; } = false;
    public double Temperature { get; set; } = 0.7;
}