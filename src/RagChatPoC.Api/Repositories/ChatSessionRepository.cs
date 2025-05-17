using Microsoft.EntityFrameworkCore;
using RagChatPoC.Api.Data;

namespace RagChatPoC.Api.Repositories;

public class ChatSessionRepository(RagDbContext context) : IChatSessionRepository
{
    public async Task<IEnumerable<ChatSession>> GetAllSessions()
    {
        var sessions = await context.ChatSessions.OrderByDescending(s => s.CreatedAt).ToListAsync();
        return sessions;
    }

    public async Task<ChatSession?> GetSession(Guid id)
    {
        var session = await context.ChatSessions.FirstOrDefaultAsync(s => s.Id == id);
        return session;
    }

    public async Task<Guid> StartNewSession(string? title)
    {
        var session = new ChatSession()
        {
            Id = Guid.NewGuid(),
            Title = title,
            CreatedAt = DateTime.UtcNow
        };

        await context.ChatSessions.AddAsync(session);
        await context.SaveChangesAsync();
        return session.Id;
    }
}