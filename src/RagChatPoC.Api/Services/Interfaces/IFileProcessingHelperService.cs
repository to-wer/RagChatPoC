using System.Text;
using RagChatPoC.Api.Models;

namespace RagChatPoC.Api.Services.Interfaces;

public interface IFileProcessingHelperService
{
    List<TextChunk> ChunkText(string content, string source, int chunkLength = 1000);
}