namespace Shuttle.Recall.EFCore.SqlServer.Storage;

public class SqlServerStorageOptions
{
    public const string SectionName = "Shuttle:EventStore:SqlServer:Storage";

    public string ConnectionStringName { get; set; } = string.Empty;
    public string Schema { get; set; } = "dbo";
    public string MigrationsHistoryTableName { get; set; } = "__StorageMigrationsHistory";
    public int CommandTimeout { get; set; } = 30;

}