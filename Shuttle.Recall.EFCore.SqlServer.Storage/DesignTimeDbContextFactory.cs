using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Shuttle.Extensions.EFCore;

namespace Shuttle.Recall.EFCore.SqlServer.Storage;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<StorageDbContext>
{
    public StorageDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddCommandLine(args)
            .Build();

        var sqlServerStorageOptions = configuration.GetSection(SqlServerStorageOptions.SectionName).Get<SqlServerStorageOptions>()!;

        var schemaOverride = configuration["SchemaOverride"];

        if (!string.IsNullOrWhiteSpace(schemaOverride))
        {
            Console.WriteLine(@$"[schema-override] : original schema = '{sqlServerStorageOptions.Schema}' / schema override = '{schemaOverride}'");

            sqlServerStorageOptions.Schema = schemaOverride;
        }

        var optionsBuilder = new DbContextOptionsBuilder<StorageDbContext>();

        optionsBuilder
            .UseSqlServer(configuration.GetConnectionString(sqlServerStorageOptions.ConnectionStringName),
                builder => builder.MigrationsHistoryTable(sqlServerStorageOptions.MigrationsHistoryTableName, sqlServerStorageOptions.Schema));

        optionsBuilder.ReplaceService<IMigrationsAssembly, SchemaMigrationsAssembly>();

        return new(Options.Create(sqlServerStorageOptions), optionsBuilder.Options);
    }
}