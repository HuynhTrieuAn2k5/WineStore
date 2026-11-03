using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliquerAspNetCore.Models.Entities;

public class Product
{
    public Guid ProductId { get; set; }
    [Required, StringLength(30)] public string ProductCode { get; set; } = string.Empty;
    [Required, StringLength(300)] public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public Guid ManufacturerId { get; set; }
    public Guid SupplierId { get; set; }
    [StringLength(100)] public string? Origin { get; set; }
    public int? VolumeMl { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? AlcoholPct { get; set; }
    [Column(TypeName = "decimal(15,0)")] public decimal SellPrice { get; set; }
    public int StockQuantity { get; set; }
    public int MinStockAlert { get; set; } = 5;
    [StringLength(1000)] public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ProductCategory? Category { get; set; }
    public Manufacturer? Manufacturer { get; set; }
    public Supplier? Supplier { get; set; }
    public ICollection<StockBatch> StockBatches { get; set; } = new List<StockBatch>();
}
