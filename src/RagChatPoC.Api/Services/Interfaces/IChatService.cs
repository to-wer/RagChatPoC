using RagChatPoC.Api.Models;

namespace RagChatPoC.Api.Services.Interfaces;

public interface IChatService
{
    Task<string?> GetAnswerAsync(string question, string context);

    Task<string?> GetAnswerAsync(string prompt);
    IAsyncEnumerable<string> GetStreamingCompletionAsync(string prompt);

}