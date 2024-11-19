using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;
using Shuttle.Extensions.EFCore;

namespace Shuttle.Recall.EFCore.SqlServer.Storage;

public class DbContextObserver :
    IPipelineObserver<OnBeforeGetStreamEvents>,
    IPipelineObserver<OnAfterGetStreamEvents>,
    IPipelineObserver<OnBeforeSavePrimitiveEvents>,
    IPipelineObserver<OnAfterSavePrimitiveEvents>,
    IPipelineObserver<OnBeforeRemoveEventStream>,
    IPipelineObserver<OnAfterRemoveEventStream>
{
    private const string DbContextStateKey = "Shuttle.Recall.Sql.Storage.DbContextObserver:DbContext";
    private const string DisposeDbContextStateKey = "Shuttle.Recall.Sql.Storage.DbContextObserver:DisposeDbContext";
    private readonly IDbContextService _dbContextService;
    private readonly IDbContextFactory<StorageDbContext> _dbContextFactory;

    public DbContextObserver(IDbContextService dbContextService, IDbContextFactory<StorageDbContext> dbContextFactory)
    {
        _dbContextService = Guard.AgainstNull(dbContextService);
        _dbContextFactory = Guard.AgainstNull(dbContextFactory);
    }

    public async Task ExecuteAsync(IPipelineContext<OnAfterGetStreamEvents> pipelineContext)
    {
        await DisposeDbContextAsync(pipelineContext);
    }

    public async Task ExecuteAsync(IPipelineContext<OnAfterRemoveEventStream> pipelineContext)
    {
        await DisposeDbContextAsync(pipelineContext);
    }

    public async Task ExecuteAsync(IPipelineContext<OnAfterSavePrimitiveEvents> pipelineContext)
    {
        await DisposeDbContextAsync(pipelineContext);
    }

    public async Task ExecuteAsync(IPipelineContext<OnBeforeGetStreamEvents> pipelineContext)
    {
        await CreateDbContextAsync(pipelineContext);
    }

    public async Task ExecuteAsync(IPipelineContext<OnBeforeRemoveEventStream> pipelineContext)
    {
        await CreateDbContextAsync(pipelineContext);
    }

    public async Task ExecuteAsync(IPipelineContext<OnBeforeSavePrimitiveEvents> pipelineContext)
    {
        await CreateDbContextAsync(pipelineContext);
    }

    private async Task CreateDbContextAsync(IPipelineContext pipelineContext)
    {
        var hasDbContext = _dbContextService.Contains<StorageDbContext>();

        Guard.AgainstNull(pipelineContext).Pipeline.State.Add(DisposeDbContextStateKey, !hasDbContext);

        if (!hasDbContext)
        {
            var storageDbContext = await _dbContextFactory.CreateDbContextAsync(pipelineContext.Pipeline.CancellationToken);

            pipelineContext.Pipeline.State.Add(DbContextStateKey, storageDbContext);

            _dbContextService.Add(storageDbContext);
        }

        await Task.CompletedTask;
    }

    private async Task DisposeDbContextAsync(IPipelineContext pipelineContext)
    {
        if (Guard.AgainstNull(pipelineContext).Pipeline.State.Get<bool>(DisposeDbContextStateKey))
        {
            await Guard.AgainstNull(pipelineContext.Pipeline.State.Get<StorageDbContext>(DbContextStateKey)).TryDisposeAsync();

            _dbContextService.Remove<StorageDbContext>();
        }

        pipelineContext.Pipeline.State.Remove(DbContextStateKey);
        pipelineContext.Pipeline.State.Remove(DisposeDbContextStateKey);

        await Task.CompletedTask;
    }
}