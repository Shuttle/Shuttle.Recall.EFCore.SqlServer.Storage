using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Shuttle.Recall.EntityFrameworkCore.SqlServer.Storage.Models;

[PrimaryKey(nameof(Id), nameof(System.Version))]
public class PrimitiveEvent
{
    public Guid Id { get; set; }

    public int Version { get; set; }

    public byte[] EventEnvelope { get; set; } = null!;

    public Guid EventId { get; set; }

    public Guid EventTypeId { get; set; }

    public long SequenceNumber { get; set; }

    public DateTime DateRegistered { get; set; }

    public Guid? CorrelationId { get; set; }
}