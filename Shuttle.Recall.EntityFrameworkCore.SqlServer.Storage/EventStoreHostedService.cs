using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Extensions.EntityFrameworkCore;

namespace Shuttle.Recall.EntityFrameworkCore.SqlServer.Storage;

public class EventStoreHostedService : IHostedService
{
    private readonly DbContextObserver _dbContextObserver;
    private readonly Type _getEventStreamPipelineType = typeof(GetEventStreamPipeline);

    private readonly IPipelineFactory _pipelineFactory;
    private readonly Type _removeEventStreamPipelineType = typeof(RemoveEventStreamPipeline);
    private readonly Type _saveEventStreamPipelineType = typeof(SaveEventStreamPipeline);

    public EventStoreHostedService(IPipelineFactory pipelineFactory, IDbContextService dbContextService, IDbContextFactory<StorageDbContext> dbContextFactory)
    {
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);

        _dbContextObserver = new(dbContextService, dbContextFactory);

        pipelineFactory.PipelineCreated += OnPipelineCreated;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _pipelineFactory.PipelineCreated -= OnPipelineCreated;

        await Task.CompletedTask;
    }

    private void OnPipelineCreated(object? sender, PipelineEventArgs e)
    {
        var pipelineType = e.Pipeline.GetType();

        if (pipelineType != _getEventStreamPipelineType &&
            pipelineType != _saveEventStreamPipelineType &&
            pipelineType != _removeEventStreamPipelineType)
        {
            return;
        }

        e.Pipeline.AddObserver(_dbContextObserver);
    }
}