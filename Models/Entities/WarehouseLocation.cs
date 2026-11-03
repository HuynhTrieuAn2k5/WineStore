using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliquerAspNetCore.Models.Entities;

public class WarehouseLocation
{
    public Guid LocationId { get; set; }
    [Required, StringLength(20)] public string LocationCode { get; set; } = string.Empty;
    [Required, StringLength(10)] public string Zone { get; set; } = string.Empty;
    [Required, StringLength(10)] public string Shelf { get; set; } = string.Empty;
    [Required, StringLength(10)] public string Compartment { get; set; } = string.Empty;
    [StringLength(500)] public string? Description { get; set; }
    public ICollection<StockBatch> StockBatches { get; set; } = new List<StockBatch>();
}
