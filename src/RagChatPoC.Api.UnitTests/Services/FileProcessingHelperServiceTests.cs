using Microsoft.Extensions.Logging;
using Moq;
using RagChatPoC.Api.Services;

namespace RagChatPoC.Api.UnitTests.Services;

public class FileProcessingHelperServiceTests
{
    [Fact]
    public void ChunkText_ShouldSplitContentIntoChunksBasedOnChunkLength()
    {
        var content =
            "This is a sample paragraph.\n\nThis is another paragraph that is longer and should be split into multiple chunks because it exceeds the chunk length.";
        var source = "example.txt";
        var chunkLength = 50;
        var logger = new Mock<ILogger<FileProcessingHelperService>>();
        var service = new FileProcessingHelperService(logger.Object);

        var chunks = service.ChunkText(content, source, chunkLength);

        Assert.Equal(4, chunks.Count);
        Assert.Equal("This is a sample paragraph.", chunks[0].Content);
        Assert.Equal("This is another paragraph that is longer and", chunks[1].Content);
        Assert.Equal("should be split into multiple chunks because it", chunks[2].Content);
    }

    [Fact]
    public void ChunkText_ShouldHandleEmptyContentGracefully()
    {
        var content = "";
        var source = "example.txt";
        var logger = new Mock<ILogger<FileProcessingHelperService>>();
        var service = new FileProcessingHelperService(logger.Object);

        var chunks = service.ChunkText(content, source);

        Assert.Empty(chunks);
    }

    [Fact]
    public void ChunkText_ShouldHandleContentWithSingleShortParagraph()
    {
        var content = "Short paragraph.";
        var source = "example.txt";
        var logger = new Mock<ILogger<FileProcessingHelperService>>();
        var service = new FileProcessingHelperService(logger.Object);

        var chunks = service.ChunkText(content, source);

        Assert.Single(chunks);
        Assert.Equal("Short paragraph.", chunks[0].Content);
        Assert.Equal(source, chunks[0].Source);
    }

    [Fact]
    public void ChunkText_ShouldSplitLongParagraphsCorrectly()
    {
        var content =
            "This is a very long paragraph that exceeds the chunk length limit. It should be split into multiple chunks.";
        var source = "example.txt";
        var chunkLength = 50;
        var logger = new Mock<ILogger<FileProcessingHelperService>>();
        var service = new FileProcessingHelperService(logger.Object);

        var chunks = service.ChunkText(content, source, chunkLength);

        Assert.Equal(3, chunks.Count);
        Assert.Equal("This is a very long paragraph that exceeds the", chunks[0].Content);
        Assert.Equal("chunk length limit. It should be split into", chunks[1].Content);
    }

    [Fact]
    public void ChunkText_ShouldHandlePdfSourceWithDifferentParagraphSeparator()
    {
        var content = "Paragraph one.\r\nParagraph two.\r\nParagraph three.";
        var source = "example.pdf";
        var chunkLength = 16;
        var logger = new Mock<ILogger<FileProcessingHelperService>>();
        var service = new FileProcessingHelperService(logger.Object);

        var chunks = service.ChunkText(content, source, chunkLength);

        Assert.Equal(3, chunks.Count);
        Assert.Equal("Paragraph one.", chunks[0].Content);
        Assert.Equal("Paragraph two.", chunks[1].Content);
        Assert.Equal("Paragraph three.", chunks[2].Content);
    }
}