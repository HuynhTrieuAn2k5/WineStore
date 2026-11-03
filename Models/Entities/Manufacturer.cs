using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliquerAspNetCore.Models.Entities;

public class Manufacturer
{
    public Guid ManufacturerId { get; set; }
    [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
    [StringLength(100)] public string? Country { get; set; }
    [StringLength(300)] public string? Website { get; set; }
    [StringLength(30)] public string? ContactPhone { get; set; }
    [StringLength(200)] public string? ContactEmail { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
