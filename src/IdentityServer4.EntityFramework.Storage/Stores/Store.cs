using IdentityServer4.EntityFramework.Storage.DbContexts;
using IdentityServer4.Storage.Stores;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer4.EntityFramework.Storage.Stores;

public class Store : IStore
{
    private readonly OperationalDbContext _context;

    public Store(OperationalDbContext context) =>
        _context = context;

    public async Task<StoreItem> Get(string key, CancellationToken ct)
    {
        var dbStoreItem = await _context
            .StoreItems
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Key == key, cancellationToken: ct);

        return new StoreItem(
            Key: dbStoreItem.Key,
            Value: dbStoreItem.Value,
            CreatedAtUtc: dbStoreItem.CreatedAtUtc,
            RemoveAtUtc: dbStoreItem.RemoveAtUtc
        );
    }

    public async Task Create(StoreItem item, CancellationToken ct)
    {
        var dbStoreItem = new DbStoreItem
        {
            Key = item.Key,
            Value = item.Value,
            CreatedAtUtc = item.CreatedAtUtc,
            RemoveAtUtc = item.RemoveAtUtc,
        };

        await _context.StoreItems.AddAsync(dbStoreItem, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task Delete(string key, CancellationToken ct)
    {
        await _context
            .StoreItems
            .Where(x => x.Key == key)
            .ExecuteDeleteAsync(ct);
    }
}