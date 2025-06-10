using Microsoft.EntityFrameworkCore;
using RagChatPoC.Api.Data;

namespace RagChatPoC.Api.Repositories;

public class CredentialsRepository(RagDbContext context) : ICredentialsRepository
{
    public async Task<Credentials?> GetById(Guid id)
    {
        return await context.Set<Credentials>().FindAsync(id);
    }

    public async Task<IEnumerable<Credentials>> GetAll()
    {
        return await context.Set<Credentials>().ToListAsync();
    }

    public async Task<Credentials> Add(Credentials credentials)
    {
        await context.Set<Credentials>().AddAsync(credentials);
        await context.SaveChangesAsync();
        return credentials;
    }

    public async Task Update(Credentials credentials)
    {
        context.Set<Credentials>().Update(credentials);
        await context.SaveChangesAsync();
    }

    public async Task Delete(Guid id)
    {
        var credentials = await GetById(id);
        if (credentials != null)
        {
            context.Set<Credentials>().Remove(credentials);
            await context.SaveChangesAsync();
        }
    }
}