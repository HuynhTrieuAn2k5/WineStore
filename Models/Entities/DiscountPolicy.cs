using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliquerAspNetCore.Models.Entities;

public class DiscountPolicy
{
    public int PolicyId { get; set; }
    [Required, StringLength(30)] public string CardType { get; set; } = string.Empty;
    [Required, StringLength(50)] public string CardNameVn { get; set; } = string.Empty;
    [Column(TypeName = "decimal(5,2)")] public decimal DiscountPct { get; set; }
    [Column(TypeName = "decimal(10,0)")] public decimal AnnualFee { get; set; }
    public int ValidityMonths { get; set; }
    public bool IsActive { get; set; } = true;
}
