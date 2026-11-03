using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliquerAspNetCore.Models.Entities;

public class DepositNotification
{
    public long NotifId { get; set; }
    public Guid DepositId { get; set; }
    public DateTime SentAt { get; set; }
    public int DaysOverdue { get; set; }
    [StringLength(20)] public string Channel { get; set; } = "system";
    public string? Message { get; set; }
    public Deposit? Deposit { get; set; }
}
