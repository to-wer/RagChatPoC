using RagChatPoC.Domain.Models;

namespace RagChatPoC.Api.Services.Interfaces;

public interface IDocumentService
{
    Task<IEnumerable<DocumentDto>> GetAllDocuments();
    Task DeleteDocument(string fileName);
}