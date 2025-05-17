using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RagChatPoC.Api.Services.Interfaces;
using RagChatPoC.Domain.Models;

namespace RagChatPoC.Api.Controllers;

[ApiController]
[Route("v{v:apiVersion}/[controller]")]
[ApiVersion(1)]
public class ChatController(IRagChatService ragChatService, 
    ILogger<ChatController> logger) : ControllerBase
{
 
    [MapToApiVersion(1)]
    [HttpPost("completions")]
    public async Task<IActionResult> PostChatCompletion([FromBody] ChatCompletionRequest request)
    {
        if (request.Stream)
        {
            Response.StatusCode = 200;
            Response.ContentType = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["X-Accel-Buffering"] = "no"; // Für NGINX (deaktiviert Pufferung)

            await foreach (var chunk in ragChatService.GetStreamingCompletionAsync(request))
            {
                // var chunk = new
                // {
                //     id = "chatcmpl-" + Guid.NewGuid().ToString("N"),
                //     @object = "chat.completion.chunk",
                //     created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                //     model = $"ollama-{request.Model}",
                //     choices = new[]
                //     {
                //         new
                //         {
                //             delta = new { content = token },
                //             index = 0,
                //             finish_reason = (string?)null
                //         }
                //     }
                // };
                var json = JsonSerializer.Serialize(chunk);
                
                await Response.WriteAsync($"data: {json}\n\n");
                await Response.Body.FlushAsync();
            }

            // Optional: Abschluss-Event senden
            await Response.WriteAsync("data: [DONE]\n\n");
            await Response.Body.FlushAsync();
            return new EmptyResult();
        }

        var result = await ragChatService.GetCompletionAsync(request);
        return Ok(result);
    }
}