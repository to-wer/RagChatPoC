using System.Text;
using RagChatPoC.Api.Services.Interfaces;

namespace RagChatPoC.Api.Services;

public class FileProcessingService(IEmbeddingService embeddingService,
    IIndexService indexService,
    ILogger<FileProcessingService> logger) : IFileProcessingService
{
    public async Task ProcessTextAsync(string sourceFileName, string text)
    {
        var chunks = ChunkText(text, sourceFileName);
        await indexService.IndexChunksAsync(chunks);
    }
    
    private List<TextChunk> ChunkText(string content, string source, int chunkLength = 1000)
        {
            logger.LogDebug("Chunking text from {Source}. Length: {Length}", source, content.Length);
            var result = new List<TextChunk>();
            string[] paragraphs = source.EndsWith(".pdf")
                ? content.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                : content.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
        
            var sb = new StringBuilder();
        
            foreach (var para in paragraphs)
            {
                if (sb.Length + para.Length > chunkLength)
                {
                    if (sb.Length > 0)
                    {
                        result.Add(new TextChunk { Source = source, Content = sb.ToString().Trim() });
                        sb.Clear();
                    }
        
                    if (para.Length > chunkLength)
                    {
                        // Absatz ist zu lang, an Wortgrenzen splitten
                        int start = 0;
                        while (start < para.Length)
                        {
                            int len = Math.Min(chunkLength, para.Length - start);
                            // Versuche, an einem Leerzeichen zu enden
                            if (start + len < para.Length)
                            {
                                int lastSpace = para.LastIndexOf(' ', start + len, len);
                                if (lastSpace > start)
                                    len = lastSpace - start;
                            }
                            string part = para.Substring(start, len).Trim();
                            if (!string.IsNullOrEmpty(part))
                                result.Add(new TextChunk { Source = source, Content = part });
                            start += len;
                        }
                        continue;
                    }
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