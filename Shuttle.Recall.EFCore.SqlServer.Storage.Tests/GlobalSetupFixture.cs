using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shuttle.Extensions.EFCore;

namespace Shuttle.Recall.EFCore.SqlServer.Storage.Tests;

[SetUpFixture]
public class GlobalSetupFixture
{
    [OneTimeSetUp]
    public void GlobalSetup()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var storageOptions = configuration.GetSection(SqlServerStorageOptions.SectionName).Get<SqlServerStorageOptions>()!;

        var dbContextOptions = new DbContextOptionsBuilder<StorageDbContext>()
            .UseSqlServer(configuration.GetConnectionString(storageOptions.ConnectionStringName), sqlServerBuilder =>
                {
                    sqlServerBuilder.CommandTimeout(300);
                    sqlServerBuilder.MigrationsHistoryTable(storageOptions.MigrationsHistoryTableName, storageOptions.Schema);
                }
            )
            .ReplaceService<IMigrationsAssembly, SchemaMigrationsAssembly>()
            .Options;

        using (var dbContext = new StorageDbContext(Options.Create(storageOptions), dbContextOptions))
        {
            dbContext.Database.Migrate();
        }
    }
}