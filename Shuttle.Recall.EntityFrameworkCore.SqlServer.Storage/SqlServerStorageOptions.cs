namespace Shuttle.Recall.EntityFrameworkCore.SqlServer.Storage;

public class SqlServerStorageOptions
{
    public const string SectionName = "Shuttle:EventStore:SqlServer:Storage";

    public string ConnectionStringName { get; set; } = string.Empty;
    public string Schema { get; set; } = "dbo";
    public int CommandTimeout { get; set; } = 30;
    public string MigrationsHistoryTableName { get; set; } = "__EFMigrationsHistory";
}