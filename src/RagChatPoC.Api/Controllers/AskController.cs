using Microsoft.AspNetCore.Mvc;
using RagChatPoC.Api.Repositories;
using RagChatPoC.Api.Services.Interfaces;

namespace RagChatPoC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AskController(IEmbeddingService embeddingService, IChatService chatService,
    IDocumentChunkRepository documentChunkRepository) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Ask([FromBody] AskRequest request)
    {
        var questionEmbedding = await embeddingService.GetEmbeddingAsync(request.Question);

        var relevantChunks = await documentChunkRepository.GetRelevantChunks(questionEmbedding);
        
        var context = string.Join("\n\n", relevantChunks.Select(c => c.ChunkText));

        var answer = await chatService.GetAnswerAsync(request.Question, context);

        return Ok(new AskResponse
        {
            Answer = answer,
            ContextUsed = context
        });
    }
}

public class AskRequest
{
    public string Question { get; set; } = default!;
}

public class AskResponse
{
    public string Answer { get; set; } = default!;
    public string ContextUsed { get; set; } = default!;
}