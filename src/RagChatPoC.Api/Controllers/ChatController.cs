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

            await foreach (var chunk in ragChatService.GetStreamingCompletion(request))
            {
                //var json = JsonSerializer.Serialize(chunk);
                
                await Response.WriteAsync($"data: {chunk}\n\n");
                await Response.Body.FlushAsync();
            }

            // Optional: Abschluss-Event senden
            await Response.WriteAsync("data: [DONE]\n\n");
            await Response.Body.FlushAsync();
            return new EmptyResult();
        }

        var result = await ragChatService.GetCompletion(request);
        return Ok(result);
    }
}