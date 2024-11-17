using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shuttle.Extensions.EntityFrameworkCore;
using Shuttle.Recall.EntityFrameworkCore.SqlServer.Storage;

namespace Shuttle.Recall.Sql.Storage.Tests;

[SetUpFixture]
public class SqlConfiguration
{
    public static IServiceCollection GetServiceCollection(IServiceCollection? serviceCollection = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var options = configuration.GetSection(SqlServerStorageOptions.SectionName).Get<SqlServerStorageOptions>()!;

        var services = (serviceCollection ?? new ServiceCollection())
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<IDbContextService, DbContextService>()
            .AddSqlServerEventStorage(builder =>
            {
                builder.Options.ConnectionStringName = options.ConnectionStringName;
                builder.Options.Schema = options.Schema;
                builder.Options.MigrationsHistoryTableName = options.MigrationsHistoryTableName;
            });

        services.AddDbContextFactory<StorageDbContext>(builder =>
        {
            var connectionString = configuration.GetConnectionString(options.ConnectionStringName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException($"Could not find a connection string called '{options.ConnectionStringName}'.");
            }

            builder.UseSqlServer(connectionString, sqlServerBuilder =>
            {
                sqlServerBuilder.CommandTimeout(300);
                sqlServerBuilder.MigrationsHistoryTable(options.MigrationsHistoryTableName, options.Schema);
            });

            builder.ReplaceService<IMigrationsAssembly, SchemaMigrationsAssembly>();
        });

        return services;
    }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", SqlClientFactory.Instance);
    }
}