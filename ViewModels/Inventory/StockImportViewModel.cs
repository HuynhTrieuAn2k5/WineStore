using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ChilliquerAspNetCore.ViewModels.Inventory;

public class StockImportViewModel
{
    [Display(Name = "Sản phẩm")]
    public Guid ProductId { get; set; }

    [Display(Name = "Vị trí kho")]
    public Guid? LocationId { get; set; }

    [Required, Display(Name = "Giá nhập")]
    public decimal ImportPrice { get; set; }

    [Required, Display(Name = "Số lượng nhập")]
    public int QuantityIn { get; set; }

    [Display(Name = "Năm sản xuất")]
    public short? ManufactureYear { get; set; }

    [Display(Name = "Hạn sử dụng")]
    public DateTime? ExpiryDate { get; set; }

    [Display(Name = "Ghi chú")]
    public string? Note { get; set; }

    public IEnumerable<SelectListItem> Products { get; set; } = [];
    public IEnumerable<SelectListItem> Locations { get; set; } = [];
}
