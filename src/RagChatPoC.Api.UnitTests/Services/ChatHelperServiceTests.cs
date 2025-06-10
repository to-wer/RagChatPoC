using System.Text.Json;
using Moq;
using RagChatPoC.Api.Repositories;
using RagChatPoC.Api.Services;
using RagChatPoC.Api.Services.Interfaces;
using RagChatPoC.Domain.Models;
using RagChatPoC.Domain.Models.RagChat;

namespace RagChatPoC.Api.UnitTests.Services;

public class ChatHelperServiceTests
{
    private readonly Mock<IEmbeddingService> _embeddingService = new Mock<IEmbeddingService>();
    private readonly Mock<IDocumentChunkRepository> _documentChunkRepository = new Mock<IDocumentChunkRepository>();

    private readonly ChatHelperService _chatHelperService;
    
    public ChatHelperServiceTests()
    {
        _chatHelperService = new ChatHelperService(_embeddingService.Object, _documentChunkRepository.Object);
    }
    
    [Fact]
    public async Task PrepareChatRequest_ShouldIncludeRelevantChunksInSystemPrompt()
    {
        var relevantChunks = new List<UsedContextChunk>
        {
            new()
            {
                Snippet = "Relevant snippet 1",
                SourceFile = "File1.pdf"
            },
            new()
            {
                Snippet = "Relevant snippet 2",
                SourceFile = "File2.pdf"
            }
        };
        var request = new ExtendedChatCompletionRequest
        {
            Messages = [new OpenAiChatMessage { Role = "user", Content = "User question" }],
            Model = "gpt-4",
            Stream = true
        };

        var result = await _chatHelperService.PrepareChatRequest(request, relevantChunks);

        Assert.NotNull(result);
        Assert.Contains("Relevant snippet 1", result.Messages.First().Content);
        Assert.Contains("Relevant snippet 2", result.Messages.First().Content);
        Assert.Equal("user", result.Messages.Last().Role);
        Assert.Equal("User question", result.Messages.Last().Content);
    }

    [Fact]
    public void GetLatestUserMessage_ShouldReturnLastUserMessage()
    {
        var request = new OpenAiChatCompletionRequest
        {
            Messages =
            [
                new OpenAiChatMessage { Role = "system", Content = "System message" },
                new OpenAiChatMessage { Role = "user", Content = "First user message" },
                new OpenAiChatMessage { Role = "user", Content = "Latest user message" }
            ]
        };

        var result = _chatHelperService.GetLatestUserMessage(request);

        Assert.NotNull(result);
        Assert.Equal("user", result.Role);
        Assert.Equal("Latest user message", result.Content);
    }

    [Fact]
    public void GetLatestUserMessage_ShouldReturnNull_WhenNoUserMessagesExist()
    {
        var request = new OpenAiChatCompletionRequest
        {
            Messages =
            [
                new OpenAiChatMessage { Role = "system", Content = "System message" },
                new OpenAiChatMessage { Role = "assistant", Content = "Assistant message" }
            ]
        };

        var result = _chatHelperService.GetLatestUserMessage(request);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetRelevantChunks_ShouldReturnChunksBasedOnQuestionEmbedding()
    {
        var chatMessage = new OpenAiChatMessage { Content = "User question", Role = "user" };
        var embedding = new[] { 0.1f, 0.2f, 0.3f };
        var embeddingJson = JsonSerializer.Serialize(embedding);
        var relevantChunks = new List<UsedContextChunk>
        {
            new()
            {
                Snippet = "Relevant snippet 1",
                SourceFile = "Fil1e1.pdf"
            },
            new()
            {
                Snippet = "Relevant snippet 2",
                SourceFile = "File1.pdf"
            }
        };

        _embeddingService.Setup(s => s.GetEmbeddingAsync(chatMessage.Content)).ReturnsAsync(embeddingJson);
        _documentChunkRepository.Setup(r => r.GetRelevantChunks(It.IsAny<string>())).ReturnsAsync(relevantChunks);


        var result = await _chatHelperService.GetRelevantChunks(chatMessage);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Snippet == "Relevant snippet 1");
        Assert.Contains(result, c => c.Snippet == "Relevant snippet 2");
    }
}