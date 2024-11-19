using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Recall.EFCore.SqlServer.Storage;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlServerEventStorage(this IServiceCollection services, Action<SqlServerStorageBuilder>? builder = null)
    {
        var sqlServerStorageBuilder = new SqlServerStorageBuilder(Guard.AgainstNull(services));

        builder?.Invoke(sqlServerStorageBuilder);

        services.TryAddSingleton<IValidateOptions<SqlServerStorageOptions>, SqlServerStorageOptionsValidator>();
        services.TryAddSingleton<IPrimitiveEventRepository, PrimitiveEventRepository>();
        services.TryAddSingleton<IEventTypeRepository, EventTypeRepository>();
        services.TryAddSingleton<IIdKeyRepository, IdKeyRepository>();

        services.AddOptions<SqlServerStorageOptions>().Configure(options =>
        {
            options.ConnectionStringName = sqlServerStorageBuilder.Options.ConnectionStringName;
            options.Schema = sqlServerStorageBuilder.Options.Schema;
            options.CommandTimeout = sqlServerStorageBuilder.Options.CommandTimeout;
        });

        services.AddDbContextFactory<StorageDbContext>((provider, dbContextFactoryBuilder) => 
        {
            var configuration = provider.GetRequiredService<IConfiguration>();

            var connectionString = configuration.GetConnectionString(sqlServerStorageBuilder.Options.ConnectionStringName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException(string.Format(Resources.ConnectionStringNameException, sqlServerStorageBuilder.Options.ConnectionStringName));
            }

            dbContextFactoryBuilder.UseSqlServer(connectionString, sqlServerOptions => { sqlServerOptions.CommandTimeout(sqlServerStorageBuilder.Options.CommandTimeout); });
        });

        services.AddHostedService<EventStoreHostedService>();

        return services;
    }
}