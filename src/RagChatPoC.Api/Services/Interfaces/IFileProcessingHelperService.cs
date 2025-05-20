using System.Text;
using RagChatPoC.Api.Models;

namespace RagChatPoC.Api.Services.Interfaces;

public interface IFileProcessingHelperService
{
    List<TextChunk> ChunkText(string content, string source, int chunkLength = 1000);
    
    // string[] SplitIntoParagraphs(string content, string source);
    //
    // bool IsTooLongToAdd(StringBuilder currentChunk, string paragraph, int chunkLength);
    //
    // void AddChunkIfNotEmpty(List<TextChunk> result, StringBuilder chunkBuilder, string source);
    //
    // void SplitLongParagraph(string paragraph, int chunkLength, List<TextChunk> result, string source);
}