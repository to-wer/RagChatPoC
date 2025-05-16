using RagApi.Data;

namespace RagApi.Repositories;

public interface IChatSessionRepository
{
    Task<IEnumerable<ChatSession>> GetAllSessions();
    Task<ChatSession?> GetSession(Guid id);
    Task<Guid> StartNewSession(string? title);
}