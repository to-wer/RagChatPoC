namespace RagChatPoC.Api.Services.Interfaces;

public interface IEmbeddingService
{
    Task<string> GetEmbeddingAsync(string text);
}