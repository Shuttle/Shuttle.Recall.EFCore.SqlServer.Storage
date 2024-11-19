using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;
using Shuttle.Extensions.EntityFrameworkCore;

namespace Shuttle.Recall.EFCore.SqlServer.Storage;

public class IdKeyRepository : IIdKeyRepository
{
    private readonly IDbContextService _dbContextService;

    public IdKeyRepository(IDbContextService dbContextService)
    {
        _dbContextService = Guard.AgainstNull(dbContextService);
    }

    public async Task AddAsync(Guid id, string key, CancellationToken cancellationToken = default)
    {
        await _dbContextService.Get<StorageDbContext>().IdKeys.AddAsync(new() { Id = id, UniqueKey = key }, cancellationToken);
    }

    public async ValueTask<bool> ContainsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _dbContextService.Get<StorageDbContext>().IdKeys.FirstOrDefaultAsync(item => item.UniqueKey == key, cancellationToken) != null;
    }

    public async ValueTask<bool> ContainsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContextService.Get<StorageDbContext>().IdKeys.FirstOrDefaultAsync(item => item.Id == id, cancellationToken) != null;
    }

    public async ValueTask<Guid?> FindAsync(string key, CancellationToken cancellationToken = default)
    {
        return (await _dbContextService.Get<StorageDbContext>().IdKeys.FirstOrDefaultAsync(item => item.UniqueKey == key, cancellationToken))?.Id;
    }

    public async Task RekeyAsync(string key, string rekey, CancellationToken cancellationToken = default)
    {
        var storageDbContext = _dbContextService.Get<StorageDbContext>();
        var model = await storageDbContext.IdKeys.FirstOrDefaultAsync(item => item.UniqueKey == key, cancellationToken);

        if (model == null)
        {
            throw new KeyNotFoundException(key);
        }

        storageDbContext.IdKeys.Remove(model);
        storageDbContext.IdKeys.Add(new() { Id = model.Id, UniqueKey = rekey });
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var storageDbContext = _dbContextService.Get<StorageDbContext>();

        var model = await storageDbContext.IdKeys.FirstOrDefaultAsync(item => item.UniqueKey == key, cancellationToken);

        if (model == null)
        {
            throw new KeyNotFoundException(key);
        }

        storageDbContext.IdKeys.Remove(model);
    }

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var storageDbContext = _dbContextService.Get<StorageDbContext>();

        var models = await storageDbContext.IdKeys.Where(item => item.Id == id).ToListAsync(cancellationToken);

        storageDbContext.IdKeys.RemoveRange(models);
    }
}