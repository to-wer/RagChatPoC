using RagChatPoC.Api.Data;

namespace RagChatPoC.Api.Repositories;

public interface ICredentialsRepository
{
    Task<Credentials?> GetById(Guid id);
    Task<IEnumerable<Credentials>> GetAll();
    Task<Credentials> Add(Credentials credentials);
    Task Update(Credentials credentials);
    Task Delete(Guid id);
}