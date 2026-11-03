using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliquerAspNetCore.Models.Entities;

public class Invoice
{
    public Guid InvoiceId { get; set; }
    [StringLength(30)] public string? InvoiceNumber { get; set; }
    public Guid CashierId { get; set; }
    public Guid? CardId { get; set; }
    [StringLength(200)] public string? CustomerName { get; set; }
    public DateTime SaleDate { get; set; }
    [StringLength(20)] public string SaleType { get; set; } = "direct";
    [Column(TypeName = "decimal(15,0)")] public decimal TotalBeforeDiscount { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal DiscountPct { get; set; }
    [Column(TypeName = "decimal(15,0)")] public decimal DiscountAmount { get; set; }
    [Column(TypeName = "decimal(15,0)")] public decimal TotalAmount { get; set; }
    [StringLength(20)] public string PaymentMethod { get; set; } = "cash";
    [StringLength(20)] public string Status { get; set; } = "completed";
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }

    public UserAccount? Cashier { get; set; }
    public MemberCard? Card { get; set; }
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
}
