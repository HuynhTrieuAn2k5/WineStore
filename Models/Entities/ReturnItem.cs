using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliquerAspNetCore.Models.Entities;

public class ReturnItem
{
    public Guid RetItemId { get; set; }
    public Guid ReturnId { get; set; }
    public Guid ProductId { get; set; }
    [Required, StringLength(300)] public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    [Column(TypeName = "decimal(15,0)")] public decimal UnitPrice { get; set; }
    [Column(TypeName = "decimal(15,0)")] public decimal Subtotal { get; set; }
    public bool ConditionOk { get; set; } = true;
    public string? Note { get; set; }
    public Return? Return { get; set; }
    public Product? Product { get; set; }
}
