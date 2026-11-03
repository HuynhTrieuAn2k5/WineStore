using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliquerAspNetCore.Models.Entities;

public class Return
{
    public Guid ReturnId { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid ProcessedBy { get; set; }
    public DateTime ReturnDate { get; set; }
    [StringLength(20)] public string ReturnType { get; set; } = "return";
    [Column(TypeName = "decimal(5,2)")] public decimal CancelFeePct { get; set; }
    [Column(TypeName = "decimal(15,0)")] public decimal CancelFeeAmt { get; set; }
    [Column(TypeName = "decimal(15,0)")] public decimal RefundAmount { get; set; }
    public string? Reason { get; set; }
    [StringLength(20)] public string Status { get; set; } = "approved";
    public Invoice? Invoice { get; set; }
    [ForeignKey("ProcessedBy")] public UserAccount? ProcessedByUser { get; set; }
    public ICollection<ReturnItem> Items { get; set; } = new List<ReturnItem>();
}
