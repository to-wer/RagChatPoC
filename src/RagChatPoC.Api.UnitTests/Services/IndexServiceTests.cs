using Moq;
using RagChatPoC.Api.Data;
using RagChatPoC.Api.Models;
using RagChatPoC.Api.Repositories;
using RagChatPoC.Api.Services;
using RagChatPoC.Api.Services.Interfaces;

namespace RagChatPoC.Api.UnitTests.Services;

public class IndexServiceTests
{
    private readonly Mock<IEmbeddingService> _mockEmbeddingService = new Mock<IEmbeddingService>();
    private readonly Mock<IDocumentChunkRepository> _mockDocumentChunkRepository = new Mock<IDocumentChunkRepository>();
    
    [Fact]
public async Task IndexChunksAsync_ShouldThrowArgumentException_WhenChunksAreNull()
{
    var service = new IndexService(_mockEmbeddingService.Object, _mockDocumentChunkRepository.Object);

    await Assert.ThrowsAsync<ArgumentNullException>(() => service.IndexChunksAsync(null));
}

[Fact]
public async Task IndexChunksAsync_ShouldThrowArgumentException_WhenChunksAreEmpty()
{
    var service = new IndexService(_mockEmbeddingService.Object, _mockDocumentChunkRepository.Object);

    await Assert.ThrowsAsync<ArgumentException>(() => service.IndexChunksAsync(Enumerable.Empty<TextChunk>()));
}

[Fact]
public async Task IndexChunksAsync_ShouldDeleteExistingChunksForSourceFile()
{
    var chunks = new List<TextChunk>
    {
        new() { Source = "file1.txt", Content = "Chunk content" }
    };
    var service = new IndexService(_mockEmbeddingService.Object, _mockDocumentChunkRepository.Object);

    await service.IndexChunksAsync(chunks);

    _mockDocumentChunkRepository.Verify(r => r.DeleteBySource("file1.txt"), Times.Once);
}

[Fact]
public async Task IndexChunksAsync_ShouldSaveChunksWithEmbeddings()
{
    var chunks = new List<TextChunk>
    {
        new() { Source = "file1.txt", Content = "Chunk content" }
    };
    _mockEmbeddingService.Setup(s => s.GetEmbeddingAsync("Chunk content")).ReturnsAsync("[1, 2, 3]");
    var service = new IndexService(_mockEmbeddingService.Object, _mockDocumentChunkRepository.Object);

    await service.IndexChunksAsync(chunks);

    _mockDocumentChunkRepository.Verify(r => r.SaveChunk(It.Is<DocumentChunk>(d =>
        d.SourceFile == "file1.txt" &&
        d.ChunkText == "Chunk content" &&
        d.EmbeddingJson == "[1, 2, 3]"
    )), Times.Once);
}

[Fact]
public async Task IndexChunksAsync_ShouldSkipChunksWithEmptyContent()
{
    var chunks = new List<TextChunk>
    {
        new() { Source = "file1.txt", Content = " " },
        new() { Source = "file1.txt", Content = string.Empty }
    };
    var service = new IndexService(_mockEmbeddingService.Object, _mockDocumentChunkRepository.Object);

    await service.IndexChunksAsync(chunks);

    _mockDocumentChunkRepository.Verify(r => r.SaveChunk(It.IsAny<DocumentChunk>()), Times.Never);
}
}