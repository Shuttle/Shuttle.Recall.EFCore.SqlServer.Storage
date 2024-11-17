using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Shuttle.Extensions.EntityFrameworkCore;

namespace Shuttle.Recall.EntityFrameworkCore.SqlServer.Storage;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<StorageDbContext>
{
    public StorageDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var sqlServerStorageOptions = configuration.GetSection(SqlServerStorageOptions.SectionName).Get<SqlServerStorageOptions>()!;

        var optionsBuilder = new DbContextOptionsBuilder<StorageDbContext>();

        optionsBuilder
            .UseSqlServer(configuration.GetConnectionString(sqlServerStorageOptions.ConnectionStringName),
                builder => builder.MigrationsHistoryTable(sqlServerStorageOptions.MigrationsHistoryTableName, sqlServerStorageOptions.Schema));

        optionsBuilder.ReplaceService<IMigrationsAssembly, SchemaMigrationsAssembly>();

        return new(Options.Create(sqlServerStorageOptions), optionsBuilder.Options);
    }
}