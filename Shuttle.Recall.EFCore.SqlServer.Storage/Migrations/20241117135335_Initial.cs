using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Shuttle.Core.Contract;
using Shuttle.Extensions.EntityFrameworkCore;

#nullable disable

namespace Shuttle.Recall.EFCore.SqlServer.Storage.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    private readonly IDbContextSchema _dbContextSchema;

    public Initial(IDbContextSchema dbContextSchema)
    {
        _dbContextSchema = Guard.AgainstNull(dbContextSchema);
    }

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: _dbContextSchema.Schema);

        migrationBuilder.CreateTable(
            name: "EventType",
            schema: _dbContextSchema.Schema,
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TypeName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EventType", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "IdKey",
            schema: _dbContextSchema.Schema,
            columns: table => new
            {
                UniqueKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_IdKey", x => x.UniqueKey);
            });

        migrationBuilder.CreateTable(
            name: "PrimitiveEvent",
            schema: _dbContextSchema.Schema,
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Version = table.Column<int>(type: "int", nullable: false),
                EventEnvelope = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EventTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SequenceNumber = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DateRegistered = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PrimitiveEvent", x => new { x.Id, x.Version });
            });

        migrationBuilder.CreateTable(
            name: "PrimitiveEventJournal",
            schema: _dbContextSchema.Schema,
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Version = table.Column<int>(type: "int", nullable: false),
                SequenceNumber = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PrimitiveEventJournal", x => new { x.Id, x.Version });
            });

        migrationBuilder.CreateIndex(
            name: "IX_EventType",
            schema: _dbContextSchema.Schema,
            table: "EventType",
            column: "TypeName",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "EventType",
            schema: _dbContextSchema.Schema);

        migrationBuilder.DropTable(
            name: "IdKey",
            schema: _dbContextSchema.Schema);

        migrationBuilder.DropTable(
            name: "PrimitiveEvent",
            schema: _dbContextSchema.Schema);

        migrationBuilder.DropTable(
            name: "PrimitiveEventJournal",
            schema: _dbContextSchema.Schema);
    }
}