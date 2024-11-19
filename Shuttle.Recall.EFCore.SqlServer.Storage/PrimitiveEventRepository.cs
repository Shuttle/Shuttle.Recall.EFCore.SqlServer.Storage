using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Extensions.EFCore;

namespace Shuttle.Recall.EFCore.SqlServer.Storage;

public class PrimitiveEventRepository : IPrimitiveEventRepository
{
    private readonly IDbContextService _dbContextService;
    private readonly IEventTypeRepository _eventTypeRepository;
    private readonly SqlServerStorageOptions _sqlServerStorageOptions;

    public PrimitiveEventRepository(IOptions<SqlServerStorageOptions> sqlServerStorageOptions, IDbContextService dbContextService, IEventTypeRepository eventTypeRepository)
    {
        _sqlServerStorageOptions = Guard.AgainstNull(Guard.AgainstNull(sqlServerStorageOptions).Value);
        _dbContextService = Guard.AgainstNull(dbContextService);
        _eventTypeRepository = Guard.AgainstNull(eventTypeRepository);
    }

    public async Task<IEnumerable<PrimitiveEvent>> GetAsync(Guid id)
    {
        return await _dbContextService.Get<StorageDbContext>().PrimitiveEvents
            .Join(_dbContextService.Get<StorageDbContext>().EventTypes, primitiveEvent => primitiveEvent.EventTypeId, eventType => eventType.Id, (primitiveEvent, eventType) => new { primitiveEvent, eventType })
            .Where(item => item.primitiveEvent.Id == id)
            .Select(item => new PrimitiveEvent
            {
                Id = item.primitiveEvent.Id,
                Version = item.primitiveEvent.Version,
                EventId = item.primitiveEvent.EventId,
                EventEnvelope = item.primitiveEvent.EventEnvelope,
                EventType = item.eventType.TypeName,
                SequenceNumber = item.primitiveEvent.SequenceNumber,
                DateRegistered = item.primitiveEvent.DateRegistered,
                CorrelationId = item.primitiveEvent.CorrelationId
            })
            .ToListAsync();
    }

    public async ValueTask<long> GetSequenceNumberAsync(Guid id)
    {
        return await _dbContextService.Get<StorageDbContext>().PrimitiveEvents
            .Where(primitiveEvent => primitiveEvent.EventId == id)
            .MaxAsync(primitiveEvent => primitiveEvent.SequenceNumber);
    }

    public async Task RemoveAsync(Guid id)
    {
        await _dbContextService.Get<StorageDbContext>().Database.ExecuteSqlRawAsync($"delete from [{_sqlServerStorageOptions.Schema}].[PrimitiveEvent] where Id = '{id}'");
    }

    public async ValueTask<long> SaveAsync(IEnumerable<PrimitiveEvent> primitiveEvents)
    {
        var events = new List<Models.PrimitiveEvent>();

        foreach (var primitiveEvent in primitiveEvents)
        {
            var eventTypeId = await _eventTypeRepository.GetIdAsync(primitiveEvent.EventType).ConfigureAwait(false);

            events.Add(new()
            {
                Id = primitiveEvent.Id,
                Version = primitiveEvent.Version,
                EventEnvelope = primitiveEvent.EventEnvelope,
                EventId = primitiveEvent.EventId,
                EventTypeId = eventTypeId,
                SequenceNumber = primitiveEvent.SequenceNumber,
                DateRegistered = primitiveEvent.DateRegistered,
                CorrelationId = primitiveEvent.CorrelationId
            });
        }

        await _dbContextService.Get<StorageDbContext>().PrimitiveEvents.AddRangeAsync(events);

        await _dbContextService.Get<StorageDbContext>().SaveChangesAsync();

        return events.Max(item => item.SequenceNumber);
    }
}