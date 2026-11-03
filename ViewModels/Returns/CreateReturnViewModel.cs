using System.ComponentModel.DataAnnotations;

namespace ChilliquerAspNetCore.ViewModels.Returns;

public class CreateReturnViewModel
{
    public Guid InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    
    [Display(Name = "Loại yêu cầu")]
    public string ReturnType { get; set; } = "return";
    
    [Display(Name = "Lý do")]
    public string? Reason { get; set; }
    
    public List<ReturnItemViewModel> Items { get; set; } = new List<ReturnItemViewModel>();
}

public class ReturnItemViewModel
{
    public Guid ItemId { get; set; } // InvoiceItem.ItemId
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int MaxQuantity { get; set; }
    
    [Display(Name = "Số lượng trả")]
    [Range(0, 1000)]
    public int ReturnQuantity { get; set; }
    
    public bool ConditionOk { get; set; } = true;
}
