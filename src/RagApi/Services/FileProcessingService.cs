using System.Text;
using RagApi.Services.Interfaces;

namespace RagApi.Services;

public class FileProcessingService(IEmbeddingService embeddingService,
    IIndexService indexService) : IFileProcessingService
{
    public async Task ProcessTextAsync(string sourceFileName, string text)
    {
        var chunks = ChunkText(text, sourceFileName);
        await indexService.IndexChunksAsync(chunks);
        // foreach (var chunk in chunks)
        // {
        //     var embedding = await embeddingService.GetEmbeddingAsync(chunk.Content);
        //     await indexService.SaveChunkAsync(sourceFileName, chunk, embedding);
        // }
    }
    
    private List<TextChunk> ChunkText(string content, string source, int chunkLength = 1000)
    {
        var result = new List<TextChunk>();
        var paragraphs = content.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();

        foreach (var para in paragraphs)
        {
            if (sb.Length + para.Length > 1000)
            {
                result.Add(new TextChunk { Source = source, Content = sb.ToString().Trim() });
                sb.Clear();
            }
            sb.AppendLine(para);
        }

        if (sb.Length > 0)
            result.Add(new TextChunk { Source = source, Content = sb.ToString().Trim() });

        return result;
    }
}

public class TextChunk
{
    public string Source { get; set; } = default!;
    public string Content { get; set; } = default!;
}