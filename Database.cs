namespace Api;
using Microsoft.EntityFrameworkCore;

// Entities
public record ApiKey(Guid Id, string Name, bool IsRoot = false);
public record StoredFile(Guid Id, string Name, string Path, string Extension, long Size, DateTime CreatedAt, DateTime UpdatedAt, string Sha512Hash, bool IsDeleted = false);

// Context
public class Database : DbContext
{
    public Database(DbContextOptions<Database> options): base(options){}
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<StoredFile> StoredFiles { get; set; }
}
