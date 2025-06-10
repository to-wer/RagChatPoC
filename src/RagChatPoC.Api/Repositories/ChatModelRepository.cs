using Microsoft.EntityFrameworkCore;
using RagChatPoC.Api.Data;

namespace RagChatPoC.Api.Repositories;

public class ChatModelRepository(RagDbContext context) : IChatModelRepository
{
    public async Task AddChatModel(ChatModel chatModel, CancellationToken cancellationToken = default)
    {
        await context.ChatModels.AddAsync(chatModel, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<ChatModel>> GetAllChatModels(CancellationToken cancellationToken = default)
    {
        return await context.ChatModels.ToListAsync(cancellationToken);
    }

    public async Task<ChatModel?> GetChatModel(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ChatModels.FindAsync([id], cancellationToken);
    }

    public async Task<ChatModel?> GetChatModel(string modelName, CancellationToken cancellationToken = default)
    {
        return await context.ChatModels.FirstOrDefaultAsync(cm => cm.ModelName == modelName, cancellationToken);
    }

    public async Task UpdateChatModel(ChatModel chatModel, CancellationToken cancellationToken = default)
    {
        context.ChatModels.Update(chatModel);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteChatModel(Guid id, CancellationToken cancellationToken = default)
    {
        var chatModel = await context.ChatModels.FindAsync([id], cancellationToken);
        if (chatModel != null)
        {
            context.ChatModels.Remove(chatModel);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}