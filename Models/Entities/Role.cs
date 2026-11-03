using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliquerAspNetCore.Models.Entities;

public class Role
{
    public int RoleId { get; set; }

    [Required, StringLength(50)]
    public string RoleName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public ICollection<UserAccount> UserAccounts { get; set; } = new List<UserAccount>();
}
