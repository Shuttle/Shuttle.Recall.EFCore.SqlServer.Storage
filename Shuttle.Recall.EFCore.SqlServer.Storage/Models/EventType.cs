using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Shuttle.Recall.EFCore.SqlServer.Storage.Models;

[Index(nameof(TypeName), IsUnique = true, Name = $"IX_{nameof(EventType)}")]
public class EventType
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(1024)]
    public string TypeName { get; set; } = null!;
}