namespace RagChatPoC.Api.Services.Interfaces;

public interface IFileProcessingService
{
    Task ProcessTextAsync(string sourceFileName, string text);
}