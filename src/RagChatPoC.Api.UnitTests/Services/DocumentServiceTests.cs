using Moq;
using RagChatPoC.Api.Repositories;
using RagChatPoC.Api.Services;
using RagChatPoC.Domain.Models;

namespace RagChatPoC.Api.UnitTests.Services;

public class DocumentServiceTests
{
    [Fact]
    public async Task GetAllDocuments_ReturnsAllDocuments()
    {
        var mockRepo = new Mock<IDocumentChunkRepository>();
        var expectedDocs = new List<DocumentDto>
        {
            new() { FileName = "doc1.pdf" },
            new() { FileName = "doc2.pdf" }
        };
        mockRepo.Setup(r => r.GetAllDocuments()).ReturnsAsync(expectedDocs);
        var service = new DocumentService(mockRepo.Object);

        var result = await service.GetAllDocuments();

        Assert.Equal(expectedDocs, result);
    }

    [Fact]
    public async Task GetAllDocuments_ReturnsEmptyList_WhenNoDocumentsExist()
    {
        var mockRepo = new Mock<IDocumentChunkRepository>();
        mockRepo.Setup(r => r.GetAllDocuments()).ReturnsAsync(new List<DocumentDto>());
        var service = new DocumentService(mockRepo.Object);

        var result = await service.GetAllDocuments();

        Assert.Empty(result);
    }

    [Fact]
    public async Task DeleteDocument_DeletesDocumentByFileName()
    {
        var mockRepo = new Mock<IDocumentChunkRepository>();
        var service = new DocumentService(mockRepo.Object);
        var fileName = "test.pdf";

        await service.DeleteDocument(fileName);

        mockRepo.Verify(r => r.DeleteDocument(fileName), Times.Once);
    }

    [Fact]
    public async Task DeleteDocument_DoesNotThrow_WhenFileNameDoesNotExist()
    {
        var mockRepo = new Mock<IDocumentChunkRepository>();
        mockRepo.Setup(r => r.DeleteDocument(It.IsAny<string>())).Returns(Task.CompletedTask);
        var service = new DocumentService(mockRepo.Object);

        var exception = await Record.ExceptionAsync(() => service.DeleteDocument("nonexistent.pdf"));

        Assert.Null(exception);
    }
}