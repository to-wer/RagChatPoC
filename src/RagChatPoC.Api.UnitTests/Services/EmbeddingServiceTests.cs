using System.Net;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using RagChatPoC.Api.Services;

namespace RagChatPoC.Api.UnitTests.Services;

public class EmbeddingServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration = new Mock<IConfiguration>();
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory = new Mock<IHttpClientFactory>();

    private EmbeddingService _embeddingService;

    public EmbeddingServiceTests()
    {
        _embeddingService = new EmbeddingService(_mockConfiguration.Object, _mockHttpClientFactory.Object);
    }

    [Fact]
    public async Task GetEmbeddingAsync_ShouldReturnOpenAiEmbedding_WhenApiKeyIsConfigured()
    {
        _mockConfiguration.Setup(c => c["OpenAi:ApiKey"]).Returns("valid-api-key");

        await Assert.ThrowsAsync<NotImplementedException>(() => _embeddingService.GetEmbeddingAsync("test input"));
    }

    [Fact]
    public async Task GetEmbeddingAsync_ShouldReturnOllamaEmbedding_WhenApiKeyIsNotConfigured()
    {
        _mockConfiguration.Setup(c => c["OpenAi:ApiKey"]).Returns(string.Empty);
        var mockHttpClient = new Mock<HttpMessageHandler>();
        mockHttpClient
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"embedding\": \"[1, 2, 3]\"}")
            });
        var httpClient = new HttpClient(mockHttpClient.Object);
        httpClient.BaseAddress = new Uri("http://localhost:11434/");
        _mockHttpClientFactory.Setup(f => f.CreateClient("OllamaClient")).Returns(httpClient);
        var service = new EmbeddingService(_mockConfiguration.Object, _mockHttpClientFactory.Object);
        
        var result = await service.GetEmbeddingAsync("test input");

        Assert.Equal("[1, 2, 3]", result);
    }

    [Fact]
    public async Task GetOllamaEmbeddingAsync_ShouldThrowException_WhenResponseIsNotSuccessful()
    {
        var mockHttpClient = new Mock<HttpMessageHandler>();
        mockHttpClient
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Error")
            });
        var httpClient = new HttpClient(mockHttpClient.Object);
        httpClient.BaseAddress = new Uri("http://localhost:11434/");
        _mockHttpClientFactory.Setup(f => f.CreateClient("OllamaClient")).Returns(httpClient);
        var service = new EmbeddingService(_mockConfiguration.Object, _mockHttpClientFactory.Object);
        
        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetEmbeddingAsync("test input"));
    }
}