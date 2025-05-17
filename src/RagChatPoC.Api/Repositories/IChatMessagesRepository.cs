using Microsoft.EntityFrameworkCore;
using RagChatPoC.Api.Data;

namespace RagChatPoC.Api.Repositories;

public interface IChatMessagesRepository
{
    Task<IEnumerable<ChatMessage>> GetAllMessages(Guid sessionId);
    Task AddMessage(ChatMessage message);
    Task<IEnumerable<ChatMessage>> GetChatHistory(Guid sessionId, int limit = 5);
}

public class ChatMessageRepository(RagDbContext context) : IChatMessagesRepository
{
    public async Task<IEnumerable<ChatMessage>> GetAllMessages(Guid sessionId)
    {
        var messages = await context.ChatMessages
            .Where(c => c.SessionId.Equals(sessionId))
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
        return messages;
    }

    public async Task AddMessage(ChatMessage message)
    {
        await context.ChatMessages.AddAsync(message);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ChatMessage>> GetChatHistory(Guid sessionId, int limit = 5)
    {
        var messages = await context.ChatMessages
            .Where(x => x.SessionId == sessionId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
        return messages;
    }
}