using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RagChatPoC.Api.Services.Interfaces;
using RagChatPoC.Domain.Models;

namespace RagChatPoC.Api.Controllers;

[ApiController]
[Route("v{v:apiVersion}/[controller]")]
[ApiVersion(1)]
public class ChatController(IRagChatService ragChatService) : ControllerBase
{
 
    [MapToApiVersion(1)]
    [HttpPost("completions")]
    public async Task<IActionResult> PostChatCompletion([FromBody] ChatCompletionRequest request)
    {
        if (request.Stream)
        {
            Response.StatusCode = 200;
            Response.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache";
            Response.Headers["X-Accel-Buffering"] = "no";

            await foreach (var chunk in ragChatService.GetStreamingCompletion(request))
            {
                await Response.WriteAsync($"data: {chunk}\n\n");
                await Response.Body.FlushAsync();
            }

            await Response.WriteAsync("data: [DONE]\n\n");
            await Response.Body.FlushAsync();
            return new EmptyResult();
        }

        var result = await ragChatService.GetCompletion(request);
        return Ok(result);
    }
}