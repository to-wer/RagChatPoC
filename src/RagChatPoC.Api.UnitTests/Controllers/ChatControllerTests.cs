using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RagChatPoC.Api.Controllers;
using RagChatPoC.Api.Services.Interfaces;
using RagChatPoC.Domain.Models;

namespace RagChatPoC.Api.UnitTests.Controllers;

public class ChatControllerTests
{
    [Fact]
    public async Task PostChatCompletion_ShouldReturnOkWithCompletionResult_WhenStreamIsFalse()
    {
        var request = new ChatCompletionRequest { Stream = false };
        var completionResult = new ChatCompletionResponse()
        {
            Choices = new List<ChatChoice>()
            {
                new ChatChoice() { Message = new ChatMessage() { Content = "Completion result" } }
            }
        };
        var mockRagChatService = new Mock<IRagChatService>();
        mockRagChatService.Setup(s => s.GetCompletion(request)).ReturnsAsync(completionResult);
        var logger = new Mock<ILogger<ChatController>>();
        var controller = new ChatController(mockRagChatService.Object, logger.Object);

        var result = await controller.PostChatCompletion(request) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(completionResult, result.Value);
    }
}