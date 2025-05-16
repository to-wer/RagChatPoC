using Microsoft.EntityFrameworkCore;

namespace RagApi.Data;

public class RagDbContext : DbContext
{
    public DbSet<DocumentChunk> Chunks { get; set; }
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    
    public RagDbContext(DbContextOptions<RagDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
    }
}