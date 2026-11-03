using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliquerAspNetCore.Models.Entities;

public class Deposit
{
    public Guid DepositId { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid CustomerId { get; set; }
    [Column(TypeName = "decimal(15,0)")] public decimal DepositAmount { get; set; }
    public DateTime DepositDate { get; set; }
    [DataType(DataType.Date)] public DateTime ExpectedPickup { get; set; }
    public DateTime? ActualPickup { get; set; }
    [StringLength(20)] public string Status { get; set; } = "pending";
    public string? Note { get; set; }
    public Invoice? Invoice { get; set; }
    public Customer? Customer { get; set; }
}
