using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ChilliquerAspNetCore.ViewModels.Products;

public class ProductFormViewModel
{
    public Guid? ProductId { get; set; }

    [Display(Name = "Mã sản phẩm")]
    public string ProductCode { get; set; } = string.Empty;

    [Required, Display(Name = "Tên sản phẩm")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Danh mục")]
    public int CategoryId { get; set; }

    [Display(Name = "Nhà sản xuất")]
    public Guid ManufacturerId { get; set; }

    [Display(Name = "Nhà cung cấp")]
    public Guid SupplierId { get; set; }

    [Display(Name = "Xuất xứ")]
    public string? Origin { get; set; }

    [Display(Name = "Dung tích ml")]
    public int? VolumeMl { get; set; }

    [Display(Name = "Nồng độ cồn %")]
    public decimal? AlcoholPct { get; set; }

    [Required, Display(Name = "Giá bán")]
    public decimal SellPrice { get; set; }

    [Display(Name = "Tồn tối thiểu")]
    public int MinStockAlert { get; set; } = 5;

    [Display(Name = "Ảnh sản phẩm")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Tải ảnh lên")]
    public Microsoft.AspNetCore.Http.IFormFile? ImageUpload { get; set; }

    public IEnumerable<SelectListItem> Categories { get; set; } = [];
    public IEnumerable<SelectListItem> Manufacturers { get; set; } = [];
    public IEnumerable<SelectListItem> Suppliers { get; set; } = [];
}
