using Microsoft.AspNetCore.Mvc;
using RagApi.Data;
using RagApi.Repositories;
using RagApi.Services.Interfaces;
using RagApi.Utils;

namespace RagApi.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController(IChatSessionRepository chatSessionRepository,
    IChatMessagesRepository chatMessagesRepository,
    IEmbeddingService embeddingService,
    IDocumentChunkRepository documentChunkRepository,
    IChatService chatService) : ControllerBase
{
    [HttpPost("sessions")]
    public async Task<IActionResult> StartNewSession()
    {
        var sessionId = await chatSessionRepository.StartNewSession(string.Empty);
        return Ok(sessionId);
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions()
    {
        var sessions = await chatSessionRepository.GetAllSessions();
        return Ok(sessions);
    }
    
    [HttpGet("sessions/{id:guid}")]
    public async Task<ActionResult<ChatSession>> GetSession(Guid id)
    {
        var session = await chatSessionRepository.GetSession(id);
        return session == null ? NotFound() : Ok(session);
    }

    [HttpPost("sessions/{sessionId:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid sessionId, [FromBody] string message)
    {
        var session = await chatSessionRepository.GetSession(sessionId);
        if (session == null)
        {
            return NotFound();
        }

        var userMessage = new ChatMessage()
        {
            SessionId = sessionId,
            IsUser = true,
            Content = message,
            CreatedAt = DateTime.UtcNow
        };

        await chatMessagesRepository.AddMessage(userMessage);
        
        var history = await chatMessagesRepository.GetChatHistory(sessionId, 5);
        
        var embedding = await embeddingService.GetEmbeddingAsync(message);

        var topChunks = await documentChunkRepository.GetRelevantChunks(embedding);
        
        var prompt = PromptBuilder.Build(history.ToList(), topChunks, message);
        
        var response = await chatService.GetAnswerAsync(prompt);
        
        var assistantMessage = new ChatMessage
        {
            SessionId = sessionId,
            IsUser = false,
            Content = response
        };
        await chatMessagesRepository.AddMessage(assistantMessage);
        return Ok(assistantMessage);
    }

    [HttpGet("sessions/{sessionId:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid sessionId)
    {
        var session = await chatSessionRepository.GetSession(sessionId);
        if (session == null)
        {
            return NotFound();
        }
        var messages = await chatMessagesRepository.GetAllMessages(sessionId);
        return Ok(messages);
    }
}