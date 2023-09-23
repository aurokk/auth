using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityServer4.EntityFramework.Storage.DbContexts;

public sealed class DbStoreItem
{
    public long SequenceNumber { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public required DateTime RemoveAtUtc { get; set; }
}

public class OperationalDbContext : DbContext
{
    public OperationalDbContext(DbContextOptions<OperationalDbContext> options)
        : base(options)
    {
    }

    public DbSet<DbStoreItem> StoreItems => Set<DbStoreItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DbStoreItemEntityTypeConfiguration).Assembly);
    }
}

public class DbStoreItemEntityTypeConfiguration : IEntityTypeConfiguration<DbStoreItem>
{
    public void Configure(EntityTypeBuilder<DbStoreItem> builder)
    {
        builder
            .HasKey(x => x.SequenceNumber);

        builder
            .HasIndex(x => x.Key)
            .IsUnique();

        builder
            .HasIndex(x => x.RemoveAtUtc);

        builder
            .Property(x => x.SequenceNumber)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder
            .Property(x => x.Key)
            .HasMaxLength(200)
            .IsRequired();

        builder
            .Property(x => x.Value)
            .IsRequired()
            .HasColumnType("text");

        builder
            .Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder
            .Property(x => x.RemoveAtUtc)
            .IsRequired();
    }
}