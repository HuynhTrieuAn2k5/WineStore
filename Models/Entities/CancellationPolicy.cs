using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliquerAspNetCore.Models.Entities;

public class CancellationPolicy
{
    public int PolicyId { get; set; }
    public int HoursLimit { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal FeePct { get; set; }
    [StringLength(500)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
