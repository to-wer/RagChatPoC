using System.Text.Json;
using RagChatPoC.Api.Data;
using RagChatPoC.Api.Repositories;
using RagChatPoC.Api.Services.Interfaces;
using RagChatPoC.Domain.Models;

namespace RagChatPoC.Api.Services;

public class DocumentService(IDocumentChunkRepository documentChunkRepository) : IDocumentService
{
    public async Task<IEnumerable<DocumentDto>> GetAllDocuments()
    {
        var documents = await documentChunkRepository.GetAllDocuments();
        return documents;
    }

    public async Task DeleteDocument(string fileName)
    {
        await documentChunkRepository.DeleteBySource(fileName);
    }
}