using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shuttle.Extensions.EFCore;

namespace Shuttle.Recall.EFCore.SqlServer.Storage.Tests;

public class IdKeyRepositoryFixture
{
    public static readonly Guid Id = new("047FF6FB-FB57-4F63-8795-99F252EDA62F");

    [Test]
    public async Task Should_be_able_to_use_repository_async()
    {
        var services = SqlConfiguration.GetServiceCollection();

        var serviceProvider = services.BuildServiceProvider();

        var dbContextService = serviceProvider.GetRequiredService<IDbContextService>();
        var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<StorageDbContext>>();
        var repository = serviceProvider.GetRequiredService<IIdKeyRepository>();
        var options = serviceProvider.GetRequiredService<IOptions<SqlServerStorageOptions>>().Value;

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            await dbContext.Database.ExecuteSqlRawAsync($"delete from [{options.Schema}].[IdKey] where Id = @Id", new SqlParameter("@Id", Id));
        }

        var keyA = $"a={Id}";
        var keyB = $"b={Id}";

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        using (dbContextService.Add(dbContext))
        {
            await repository.AddAsync(Id, keyA);
            await dbContext.SaveChangesAsync();
        }

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        using (dbContextService.Add(dbContext))
        {
            await repository.AddAsync(Id, keyA);

            var ex = Assert.ThrowsAsync<DbUpdateException>(async () => await dbContext.SaveChangesAsync());

            Assert.That(ex, Is.Not.Null);
            Assert.That(ex!.InnerException, Is.TypeOf<SqlException>(), $"Should not be able to add duplicate key / id = {Id} / key = '{keyA}' / (ensure that your implementation throws a `DuplicateKeyException`)");
            Assert.That(ex!.InnerException!.Message, Does.Contain("Violation of PRIMARY KEY constraint"));
        }

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        using (dbContextService.Add(dbContext))
        {
            var id = await repository.FindAsync(keyA);

            Assert.That(id, Is.Not.Null, $"Should be able to retrieve the id of the associated key / id = {Id} / key = '{keyA}'");
            Assert.That(id, Is.EqualTo(Id), $"Should be able to retrieve the correct id of the associated key / id = {Id} / key = '{keyA}' / id retrieved = {id}");

            Assert.That(await repository.FindAsync(keyB), Is.Null, $"Should not be able to get id of non-existent / id = {Id} / key = '{keyB}'");

            await repository.RemoveAsync(Id);
            await dbContext.SaveChangesAsync();

            Assert.That(await repository.FindAsync(keyA), Is.Null, $"Should be able to remove association using id (was not removed) / id = {Id} / key = '{keyA}'");
        }

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        using (dbContextService.Add(dbContext))
        {
            await repository.AddAsync(Id, keyA);
            await dbContext.SaveChangesAsync();

            await repository.RemoveAsync(keyA);
            await dbContext.SaveChangesAsync();

            Assert.That(await repository.FindAsync(keyA), Is.Null, $"Should be able to remove association using key (was not removed) / id = {Id} / key = '{keyA}'");

            Assert.That(await repository.ContainsAsync(keyA), Is.False, $"Should not contain key A / key = '{keyA}'");
            Assert.That(await repository.ContainsAsync(keyB), Is.False, $"Should not contain key B / key = '{keyB}'");

            await repository.AddAsync(Id, keyB);
            await dbContext.SaveChangesAsync();

            Assert.That(await repository.ContainsAsync(keyA), Is.False, $"Should not contain key A / key = '{keyA}'");
            Assert.That(await repository.ContainsAsync(keyB), Is.True, $"Should contain key B / key = '{keyB}'");

            await repository.RekeyAsync(keyB, keyA);
            await dbContext.SaveChangesAsync();

            Assert.That(await repository.ContainsAsync(keyA), Is.True, $"Should contain key A / key = '{keyA}'");
            Assert.That(await repository.ContainsAsync(keyB), Is.False, $"Should not contain key B / key = '{keyB}'");
        }

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        using (dbContextService.Add(dbContext))
        {
            await repository.AddAsync(Id, keyB);
            await dbContext.SaveChangesAsync();

            Assert.That(await repository.ContainsAsync(keyA), Is.True, $"Should contain key A / key = '{keyA}'");
            Assert.That(await repository.ContainsAsync(keyB), Is.True, $"Should contain key B / key = '{keyB}'");
        }

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        using (dbContextService.Add(dbContext))
        {
            await repository.RekeyAsync(keyA, keyB);

            var ex = Assert.ThrowsAsync<DbUpdateException>(async () => await dbContext.SaveChangesAsync());

            Assert.That(ex, Is.Not.Null);
            Assert.That(ex!.InnerException, Is.TypeOf<SqlException>());
            Assert.That(ex!.InnerException!.Message, Does.Contain("Violation of PRIMARY KEY constraint"));
        }

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        using (dbContextService.Add(dbContext))
        {
            await repository.RemoveAsync(Id);
            await dbContext.SaveChangesAsync();

            Assert.That(await repository.ContainsAsync(keyA), Is.False, $"Should not contain key A / key = '{keyA}'");
            Assert.That(await repository.ContainsAsync(keyB), Is.False, $"Should not contain key B / key = '{keyB}'");
        }
    }
}