namespace Api;
using Microsoft.EntityFrameworkCore;

// Entities
public record ApiKey(Guid Id, string Name, bool IsRoot = false);

// Context
public class Database : DbContext
{
    public Database(DbContextOptions<Database> options): base(options){}
    public DbSet<ApiKey> ApiKeys { get; set; }
}
