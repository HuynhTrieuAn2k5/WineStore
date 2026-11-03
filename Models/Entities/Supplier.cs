using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliquerAspNetCore.Models.Entities;

public class Supplier
{
    public Guid SupplierId { get; set; }
    [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
    [StringLength(500)] public string? Address { get; set; }
    [StringLength(30)] public string? Phone { get; set; }
    [StringLength(200)] public string? Email { get; set; }
    [StringLength(20)] public string? TaxCode { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
