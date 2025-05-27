using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RagChatPoC.Api.Controllers;
using RagChatPoC.Api.Services.Interfaces;
using RagChatPoC.Domain.Models;

namespace RagChatPoC.Api.UnitTests.Controllers;

public class ChatControllerTests
{
    private readonly Mock<IRagChatService> _mockRagChatService = new Mock<IRagChatService>();

    [Fact]
    public async Task PostChatCompletion_WhenStreamFalse_ReturnsOkWithResponse()
    {
        // Arrange
        var request = new ChatCompletionRequest { Stream = false };
        var expectedResponse = new ChatCompletionResponse
        {
            Id = "chatcmpl-123",
            Model = "test-model",
            Choices = new List<ChatChoice> { new ChatChoice() }
        };

        _mockRagChatService
            .Setup(s => s.GetCompletion(request))
            .ReturnsAsync(expectedResponse);

        var controller = new ChatController(_mockRagChatService.Object);

        // Act
        var result = await controller.PostChatCompletion(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actual = Assert.IsType<ChatCompletionResponse>(okResult.Value);
        Assert.Equal("chatcmpl-123", actual.Id);
        Assert.Equal("test-model", actual.Model);
        Assert.Single(actual.Choices);
    }

    [Fact]
    public async Task PostChatCompletion_WhenStreamTrue_WritesToResponse()
    {
        // Arrange
        var request = new ChatCompletionRequest { Stream = true };
        var chunks = new List<string> { "chunk 1", "chunk 2" };

        _mockRagChatService
            .Setup(s => s.GetStreamingCompletion(request))
            .Returns(MockStreaming(chunks));

        var controller = new ChatController(_mockRagChatService.Object);

        var responseBody = new MemoryStream();
        var context = new DefaultHttpContext();
        context.Response.Body = responseBody;
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };

        // Act
        var result = await controller.PostChatCompletion(request);

        // Assert
        Assert.IsType<EmptyResult>(result);
        responseBody.Position = 0;
        var bodyText = await new StreamReader(responseBody).ReadToEndAsync();

        foreach (var chunk in chunks)
        {
            Assert.Contains($"data: {chunk}", bodyText);
        }

        Assert.Contains("data: [DONE]", bodyText);

        Assert.Equal("text/event-stream", context.Response.ContentType);
        Assert.Equal("no-cache", context.Response.Headers["Cache-Control"]);
        Assert.Equal("no", context.Response.Headers["X-Accel-Buffering"]);
    }


    private static async IAsyncEnumerable<string> MockStreaming(IEnumerable<string> chunks)
    {
        foreach (var chunk in chunks)
        {
            await Task.Yield(); // Simulate async work
            yield return chunk;
        }
    }
}