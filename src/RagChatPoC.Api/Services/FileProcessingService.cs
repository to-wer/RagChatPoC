using System.Text;
using RagChatPoC.Api.Models;
using RagChatPoC.Api.Services.Interfaces;

namespace RagChatPoC.Api.Services;

public class FileProcessingService(
    IIndexService indexService,
    IFileProcessingHelperService fileProcessingHelperService) : IFileProcessingService
{
    public async Task ProcessTextAsync(string sourceFileName, string text)
    {
        var chunks = fileProcessingHelperService.ChunkText(text, sourceFileName);
        await indexService.IndexChunksAsync(chunks);
    }
}