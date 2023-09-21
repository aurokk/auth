using Microsoft.EntityFrameworkCore;

namespace IdentityServer4.EntityFramework.Storage.DbContexts;

public sealed class DbStoreItem
{
    public long Id { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public required DateTime RemoveAtUtc { get; set; }
}

public class OperationalDbContext : DbContext
{
    public DbSet<DbStoreItem> StoreItems => Set<DbStoreItem>();
}