using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;
using Shuttle.Extensions.EntityFrameworkCore;

namespace Shuttle.Recall.EntityFrameworkCore.SqlServer.Storage;

public class EventTypeRepository : IEventTypeRepository
{
    private readonly IDbContextService _dbContextService;
    private readonly Dictionary<string, Guid> _cache = new();
    private readonly SemaphoreSlim _lock = new(1,1);

    public EventTypeRepository(IDbContextService dbContextService)
    {
        _dbContextService = Guard.AgainstNull(dbContextService);
    }

    public async Task<Guid> GetIdAsync(string typeName, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNullOrEmptyString(typeName);

        await _lock.WaitAsync(cancellationToken);

        try
        {
            var key = typeName.ToLower();

            if (!_cache.ContainsKey(key))
            {
                var model = await _dbContextService.Get<StorageDbContext>().EventTypes.FirstOrDefaultAsync(item => item.TypeName.Equals(typeName), cancellationToken: cancellationToken);

                if (model == null)
                {
                    model = new()
                    {
                        Id = Guid.NewGuid(),
                        TypeName = typeName
                    };

                    _dbContextService.Get<StorageDbContext>().EventTypes.Add(model);
                }

                _cache.Add(key, model.Id);
            }

            return _cache[key];
        }
        finally
        {
            _lock.Release();
        }
    }
}