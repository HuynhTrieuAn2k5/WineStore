using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliquerAspNetCore.Models.Entities;

public class StockBatch
{
    public Guid BatchId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? LocationId { get; set; }
    [Column(TypeName = "decimal(15,0)")] public decimal ImportPrice { get; set; }
    public int QuantityIn { get; set; }
    public int QuantityRemain { get; set; }
    public short? ManufactureYear { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime ImportDate { get; set; }
    public Guid? ImportedBy { get; set; }
    [StringLength(20)] public string Status { get; set; } = "active";
    public string? Note { get; set; }

    public Product? Product { get; set; }
    public WarehouseLocation? Location { get; set; }
    [ForeignKey("ImportedBy")] public UserAccount? ImportedByUser { get; set; }
}
