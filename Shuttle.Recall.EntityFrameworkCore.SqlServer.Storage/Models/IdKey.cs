using System.ComponentModel.DataAnnotations;

namespace Shuttle.Recall.EntityFrameworkCore.SqlServer.Storage.Models;

public class IdKey
{
    [Key]
    public string UniqueKey { get; set; } = null!;
    public Guid Id { get; set; }
}