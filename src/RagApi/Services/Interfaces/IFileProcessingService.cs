namespace RagApi.Services.Interfaces;

public interface IFileProcessingService
{
    Task ProcessTextAsync(string sourceFileName, string text);
}