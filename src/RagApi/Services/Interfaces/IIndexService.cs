namespace RagApi.Services.Interfaces;

public interface IIndexService
{
    Task SaveChunkAsync(string sourceFileName, object chunk, object embedding);
    Task IndexChunksAsync(IEnumerable<TextChunk> chunks);

}