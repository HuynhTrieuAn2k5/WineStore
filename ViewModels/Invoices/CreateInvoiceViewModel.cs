using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ChilliquerAspNetCore.ViewModels.Invoices;

public class CreateInvoiceViewModel
{
    [Display(Name = "Tên khách hàng")]
    public string? CustomerName { get; set; }

    [Display(Name = "Thẻ thành viên")]
    public Guid? CardId { get; set; }

    [Display(Name = "Phương thức thanh toán")]
    public string PaymentMethod { get; set; } = "cash";

    [Display(Name = "Ghi chú")]
    public string? Note { get; set; }

    public List<InvoiceItemViewModel> Items { get; set; } = new List<InvoiceItemViewModel>();

    public IEnumerable<SelectListItem> Products { get; set; } = [];
    public IEnumerable<SelectListItem> MemberCards { get; set; } = [];
}

public class InvoiceItemViewModel
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}
