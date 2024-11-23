using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Shuttle.Recall.Tests;

namespace Shuttle.Recall.EFCore.SqlServer.Storage.Tests;

public class StorageFixture : RecallFixture
{
    [Test]
    public async Task Should_be_able_to_exercise_event_store_async()
    {
        var services = SqlConfiguration.GetServiceCollection(new ServiceCollection().AddSingleton(new Mock<IProjectionRepository>().Object));

        var serviceProvider = services.BuildServiceProvider();

        var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<StorageDbContext>>();
        var options = serviceProvider.GetRequiredService<IOptions<SqlServerStorageOptions>>().Value;

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            await dbContext.Database.ExecuteSqlRawAsync($"delete from [{options.Schema}].[PrimitiveEvent] where Id in ('{OrderId}', '{OrderProcessId}')");
            await dbContext.Database.ExecuteSqlRawAsync($"delete from [{options.Schema}].[PrimitiveEventJournal] where Id in ('{OrderId}', '{OrderProcessId}')");
        }

        await ExerciseStorageAsync(services);
    }
}