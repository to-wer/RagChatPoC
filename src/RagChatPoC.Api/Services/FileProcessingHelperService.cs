using System.Text;
using RagChatPoC.Api.Models;
using RagChatPoC.Api.Services.Interfaces;

namespace RagChatPoC.Api.Services;

public class FileProcessingHelperService(ILogger<FileProcessingHelperService> logger) : IFileProcessingHelperService
{
    public List<TextChunk> ChunkText(string content, string source, int chunkLength = 1000)
    {
        logger.LogDebug("Chunking text from {Source}. Length: {Length}", source, content.Length);

        var result = new List<TextChunk>();
        var paragraphs = SplitIntoParagraphs(content, source);

        var currentChunk = new StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            if (IsTooLongToAdd(currentChunk, paragraph, chunkLength))
            {
                AddChunkIfNotEmpty(result, currentChunk, source);

                if (paragraph.Length > chunkLength)
                {
                    SplitLongParagraph(paragraph, chunkLength, result, source);
                    continue;
                }
            }

            currentChunk.AppendLine(paragraph);
        }

        AddChunkIfNotEmpty(result, currentChunk, source);

        return result;
    }

    private string[] SplitIntoParagraphs(string content, string source)
    {
        return source.EndsWith(".pdf")
            ? content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
            : content.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
    }

    private bool IsTooLongToAdd(StringBuilder currentChunk, string paragraph, int chunkLength)
    {
        return currentChunk.Length + paragraph.Length > chunkLength;
    }

    private void AddChunkIfNotEmpty(List<TextChunk> result, StringBuilder chunkBuilder, string source)
    {
        if (chunkBuilder.Length == 0)
            return;

        result.Add(new TextChunk
        {
            Source = source,
            Content = chunkBuilder.ToString().Trim()
        });

        chunkBuilder.Clear();
    }

    private void SplitLongParagraph(string paragraph, int chunkLength, List<TextChunk> result, string source)
    {
        int start = 0;

        while (start < paragraph.Length)
        {
            int remainingLength = paragraph.Length - start;
            int length = Math.Min(chunkLength, remainingLength);

            if (start + length < paragraph.Length)
            {
                int lastSpace = paragraph.LastIndexOf(' ', start + length, length);
                if (lastSpace > start)
                    length = lastSpace - start;
            }

            string part = paragraph.Substring(start, length).Trim();
            if (!string.IsNullOrEmpty(part))
            {
                result.Add(new TextChunk
                {
                    Source = source,
                    Content = part
                });
            }

            start += length;
        }
    }
}