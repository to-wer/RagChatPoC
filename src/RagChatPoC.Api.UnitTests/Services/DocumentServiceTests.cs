using Moq;
using RagChatPoC.Api.Repositories;
using RagChatPoC.Api.Services;
using RagChatPoC.Api.Services.Interfaces;
using RagChatPoC.Domain.Models;

namespace RagChatPoC.Api.UnitTests.Services;

public class DocumentServiceTests
{
    private readonly Mock<IDocumentChunkRepository> _documentChunkRepository = new();
    private readonly Mock<IFileProcessingHelperService> _fileProcessingHelperService = new();
    private readonly Mock<IEmbeddingService> _embeddingService = new();

    private readonly DocumentService _documentService;

    public DocumentServiceTests()
    {
        _documentService = new DocumentService(_documentChunkRepository.Object,
            _fileProcessingHelperService.Object, _embeddingService.Object);
    }

    [Fact]
    public async Task GetAllDocuments_ReturnsAllDocuments()
    {
        var expectedDocs = new List<DocumentDto>
        {
            new() { FileName = "doc1.pdf" },
            new() { FileName = "doc2.pdf" }
        };
        _documentChunkRepository.Setup(r => r.GetAllDocuments()).ReturnsAsync(expectedDocs);

        var result = await _documentService.GetAllDocuments();

        Assert.Equal(expectedDocs, result);
    }

    [Fact]
    public async Task GetAllDocuments_ReturnsEmptyList_WhenNoDocumentsExist()
    {
        _documentChunkRepository.Setup(r => r.GetAllDocuments()).ReturnsAsync(new List<DocumentDto>());

        var result = await _documentService.GetAllDocuments();

        Assert.Empty(result);
    }

    [Fact]
    public async Task DeleteDocument_DeletesDocumentByFileName()
    {
        var fileName = "test.pdf";

        await _documentService.DeleteDocument(fileName);

        _documentChunkRepository.Verify(r => r.DeleteBySource(fileName), Times.Once);
    }

    [Fact]
    public async Task DeleteDocument_DoesNotThrow_WhenFileNameDoesNotExist()
    {
        _documentChunkRepository.Setup(r => r.DeleteBySource(It.IsAny<string>())).Returns(Task.CompletedTask);

        var exception = await Record.ExceptionAsync(() => _documentService.DeleteDocument("nonexistent.pdf"));

        Assert.Null(exception);
    }
}