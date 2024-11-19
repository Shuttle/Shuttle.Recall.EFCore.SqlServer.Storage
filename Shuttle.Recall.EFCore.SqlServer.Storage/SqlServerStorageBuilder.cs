using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Recall.EFCore.SqlServer.Storage;

public class SqlServerStorageBuilder
{
    private SqlServerStorageOptions _sqlServerStorageOptions = new();

    public SqlServerStorageBuilder(IServiceCollection services)
    {
        Services = Guard.AgainstNull(services);
    }

    public SqlServerStorageOptions Options
    {
        get => _sqlServerStorageOptions;
        set => _sqlServerStorageOptions = Guard.AgainstNull(value);
    }

    public IServiceCollection Services { get; }
}