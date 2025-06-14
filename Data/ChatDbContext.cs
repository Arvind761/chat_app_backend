using Microsoft.EntityFrameworkCore;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

    public DbSet<Message> Messages { get; set; }
}
