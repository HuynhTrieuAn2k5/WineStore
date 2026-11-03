using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliquerAspNetCore.Models.Entities;

public class InvoiceItem
{
    public Guid ItemId { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid ProductId { get; set; }
    [Required, StringLength(300)] public string ProductName { get; set; } = string.Empty;
    [Column(TypeName = "decimal(15,0)")] public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    [Column(TypeName = "decimal(15,0)")] public decimal Subtotal { get; set; }

    public Invoice? Invoice { get; set; }
    public Product? Product { get; set; }
}
