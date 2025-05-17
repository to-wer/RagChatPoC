using System.ComponentModel.DataAnnotations;

namespace RagChatPoC.Domain.Models;

public class DocumentDto
{
    [StringLength(128)]
    public required string FileName { get; set; }

    public DateTime? CreatedAt { get; set; }
    public int ChunkCount { get; set; }
}