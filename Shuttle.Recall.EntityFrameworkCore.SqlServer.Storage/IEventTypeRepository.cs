namespace Shuttle.Recall.EntityFrameworkCore.SqlServer.Storage;

public interface IEventTypeRepository
{
    Task<Guid> GetIdAsync(string typeName, CancellationToken cancellationToken = default);
}