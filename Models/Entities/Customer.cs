using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliquerAspNetCore.Models.Entities;

public class Customer
{
    public Guid CustomerId { get; set; }
    [Required, StringLength(20)] public string Cccd { get; set; } = string.Empty;
    [Required, StringLength(200)] public string FullName { get; set; } = string.Empty;
    [DataType(DataType.Date)] public DateTime DateOfBirth { get; set; }
    [StringLength(10)] public string? Gender { get; set; }
    [StringLength(500)] public string? Address { get; set; }
    [StringLength(20)] public string? Phone { get; set; }
    [StringLength(200)] public string? Email { get; set; }
    [StringLength(200)] public string? Occupation { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public MemberCard? MemberCard { get; set; }
}
