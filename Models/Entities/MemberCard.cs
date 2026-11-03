using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliquerAspNetCore.Models.Entities;

public class MemberCard
{
    public Guid CardId { get; set; }
    [StringLength(20)] public string? CardNumber { get; set; }
    public Guid CustomerId { get; set; }
    [Required, StringLength(30)] public string CardType { get; set; } = "silver";
    [DataType(DataType.Date)] public DateTime IssueDate { get; set; }
    [DataType(DataType.Date)] public DateTime ExpiryDate { get; set; }
    public int RenewalCount { get; set; }
    [Column(TypeName = "decimal(10,0)")] public decimal FeePaid { get; set; }
    public Guid? IssuedBy { get; set; }
    [StringLength(20)] public string Status { get; set; } = "active";
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Customer? Customer { get; set; }
    [ForeignKey("IssuedBy")] public UserAccount? IssuedByUser { get; set; }
}
