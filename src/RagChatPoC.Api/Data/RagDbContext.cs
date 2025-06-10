using Microsoft.EntityFrameworkCore;

namespace RagChatPoC.Api.Data;

public class RagDbContext : DbContext
{
    public DbSet<DocumentChunk> Chunks { get; set; }
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<ChatModel> ChatModels { get; set; }
    public DbSet<Credentials> Credentials { get; set; }
    
    public RagDbContext(DbContextOptions<RagDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
    }
}