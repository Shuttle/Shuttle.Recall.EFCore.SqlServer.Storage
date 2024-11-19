using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Extensions.EFCore;
using Shuttle.Recall.EFCore.SqlServer.Storage.Models;

namespace Shuttle.Recall.EFCore.SqlServer.Storage;

public class StorageDbContext : DbContext, IDbContextSchema
{
    private readonly SqlServerStorageOptions _sqlServerStorageOptions;

    public StorageDbContext(IOptions<SqlServerStorageOptions> sqlServerStorageOptions, DbContextOptions<StorageDbContext> options) : base(options)
    {
        _sqlServerStorageOptions = Guard.AgainstNull(Guard.AgainstNull(sqlServerStorageOptions).Value);
    }

    public DbSet<EventType> EventTypes { get; set; } = null!;
    public DbSet<IdKey> IdKeys { get; set; } = null!;

    public DbSet<Models.PrimitiveEvent> PrimitiveEvents { get; set; } = null!;
    public DbSet<PrimitiveEventJournal> PrimitiveEventJournals { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(_sqlServerStorageOptions.Schema);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            entityType.SetTableName(entityType.DisplayName());
        }

        modelBuilder.Entity<Models.PrimitiveEvent>()
            .Property(e => e.DateRegistered)
            .HasDefaultValueSql("GETUTCDATE()");
    }

    public string Schema => _sqlServerStorageOptions.Schema;
}