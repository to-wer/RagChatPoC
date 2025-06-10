using RagChatPoC.Api.Data;

namespace RagChatPoC.Api.Repositories;

public interface IChatModelRepository
{
    Task AddChatModel(ChatModel chatModel, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChatModel>> GetAllChatModels(CancellationToken cancellationToken = default);
    Task<ChatModel?> GetChatModel(Guid id, CancellationToken cancellationToken = default);
    Task<ChatModel?> GetChatModel(string modelName, CancellationToken cancellationToken = default);
    Task UpdateChatModel(ChatModel chatModel, CancellationToken cancellationToken = default);
    Task DeleteChatModel(Guid id, CancellationToken cancellationToken = default);
}