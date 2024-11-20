namespace Shuttle.Recall.EFCore.SqlServer.Storage;

public interface IPrimitiveEventQuery
{
    Task<IEnumerable<Models.PrimitiveEvent>> SearchAsync(Models.PrimitiveEvent.Specification specification);
}