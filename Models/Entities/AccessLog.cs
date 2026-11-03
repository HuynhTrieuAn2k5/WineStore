using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliquerAspNetCore.Models.Entities;

public class AccessLog
{
    public long LogId { get; set; }
    public Guid? UserId { get; set; }
    public DateTime LoginAt { get; set; }
    [StringLength(45)] public string? IpAddress { get; set; }
    [Required, StringLength(20)] public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public UserAccount? User { get; set; }
}
