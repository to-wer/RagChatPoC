using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RagChatPoC.Api.Controllers;
using RagChatPoC.Api.Services.Interfaces;
using RagChatPoC.Domain.Models;

namespace RagChatPoC.Api.UnitTests.Controllers;

public class DocumentsControllerTests
{
    [Fact]
    public async Task GetDocuments_ShouldReturnAllDocuments()
    {
        var documents = new List<DocumentDto>
            { new DocumentDto { FileName = "file1.txt" }, new DocumentDto { FileName = "file2.pdf" } };
        var mockDocumentService = new Mock<IDocumentService>();
        mockDocumentService.Setup(s => s.GetAllDocuments()).ReturnsAsync(documents);
        var controller = new DocumentsController(null, mockDocumentService.Object);

        var result = await controller.GetDocuments() as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(documents, result.Value);
    }

    [Fact]
    public async Task UploadFile_ShouldReturnBadRequest_WhenFileIsNull()
    {
        var controller = new DocumentsController(null, null);

        var result = await controller.UploadFile(null) as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Keine Datei hochgeladen", result.Value);
    }

    [Fact]
    public async Task UploadFile_ShouldProcessTextFile()
    {
        var mockFileProcessingService = new Mock<IFileProcessingService>();
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("file.txt");
        fileMock.Setup(f => f.Length).Returns(100);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[100]));
        var controller = new DocumentsController(mockFileProcessingService.Object, null);

        var result = await controller.UploadFile(fileMock.Object) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("Datei indexiert", result.Value);
        mockFileProcessingService.Verify(s => s.ProcessTextAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

// [Fact]
// public async Task UploadFile_ShouldProcessPdfFile()
// {
//     var mockFileProcessingService = new Mock<IFileProcessingService>();
//     var fileMock = new Mock<IFormFile>();
//     fileMock.Setup(f => f.FileName).Returns("file.pdf");
//     fileMock.Setup(f => f.Length).Returns(100);
//     fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[100]));
//     var controller = new DocumentsController(mockFileProcessingService.Object, null);
//
//     var result = await controller.UploadFile(fileMock.Object) as OkObjectResult;
//
//     Assert.NotNull(result);
//     Assert.Equal(200, result.StatusCode);
//     Assert.Equal("Datei indexiert", result.Value);
//     mockFileProcessingService.Verify(s => s.ProcessTextAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
// }

    [Fact]
    public async Task UploadZip_ShouldReturnBadRequest_WhenZipFileIsNull()
    {
        var controller = new DocumentsController(null, null);

        var result = await controller.UploadZip(null) as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Keine Datei hochgeladen", result.Value);
    }

    [Fact]
    public async Task DeleteDocument_ShouldReturnOk_WhenDocumentIsDeleted()
    {
        var mockDocumentService = new Mock<IDocumentService>();
        mockDocumentService.Setup(s => s.DeleteDocument(It.IsAny<string>())).Returns(Task.CompletedTask);
        var controller = new DocumentsController(null, mockDocumentService.Object);

        var result = await controller.DeleteDocument("file.txt") as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("Datei gelöscht", result.Value);
    }
}