using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliquerAspNetCore.Models.Entities;

public class ProductCategory
{
    public int CategoryId { get; set; }
    [Required, StringLength(100)] public string CategoryName { get; set; } = string.Empty;
    [StringLength(500)] public string? Description { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
