using ChilliquerAspNetCore.Data;
using ChilliquerAspNetCore.Filters;
using ChilliquerAspNetCore.Models.Entities;
using ChilliquerAspNetCore.ViewModels.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ChilliquerAspNetCore.Controllers;

[RequireRole("admin", "manager")]
public class InventoryController : Controller
{
    private readonly ChilliquerDbContext _context;
    public InventoryController(ChilliquerDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var batches = await _context.StockBatches
            .Include(x => x.Product)
            .Include(x => x.Location)
            .OrderByDescending(x => x.ImportDate)
            .ToListAsync();
        return View(batches);
    }

    public async Task<IActionResult> Import() => View(await BuildImportModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(StockImportViewModel model)
    {
        if (!ModelState.IsValid) return View(await BuildImportModel(model));

        if (model.QuantityIn <= 0)
        {
            ModelState.AddModelError("QuantityIn", "Số lượng nhập phải lớn hơn 0.");
            return View(await BuildImportModel(model));
        }

        var product = await _context.Products.FindAsync(model.ProductId);
        if (product == null)
        {
            ModelState.AddModelError("", "Sản phẩm không tồn tại.");
            return View(await BuildImportModel(model));
        }

        var userIdText = HttpContext.Session.GetString("UserId");
        Guid? userId = Guid.TryParse(userIdText, out var parsed) ? parsed : null;

        var batch = new StockBatch
        {
            ProductId = model.ProductId,
            LocationId = model.LocationId,
            ImportPrice = model.ImportPrice,
            QuantityIn = model.QuantityIn,
            QuantityRemain = model.QuantityIn,
            ManufactureYear = model.ManufactureYear,
            ExpiryDate = model.ExpiryDate,
            ImportDate = DateTime.Today,
            ImportedBy = userId,
            Status = "active",
            Note = model.Note
        };

        _context.StockBatches.Add(batch);
        await _context.SaveChangesAsync();

        // Reload product to get the latest StockQuantity (updated by SQL trigger)
        await _context.Entry(product).ReloadAsync();

        TempData["SuccessMessage"] = $"Đã nhập {model.QuantityIn} sản phẩm '{product.Name}'. Tồn kho hiện tại: {product.StockQuantity}";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> StockWarning()
    {
        var products = await _context.Products
            .Include(x => x.Category)
            .Where(x => x.IsActive && x.StockQuantity <= x.MinStockAlert)
            .OrderBy(x => x.StockQuantity)
            .ToListAsync();
        return View(products);
    }

    private async Task<StockImportViewModel> BuildImportModel(StockImportViewModel? model = null)
    {
        model ??= new StockImportViewModel();
        model.Products = await _context.Products.Where(x => x.IsActive).OrderBy(x => x.Name)
            .Select(x => new SelectListItem($"{x.ProductCode} - {x.Name} (Tồn: {x.StockQuantity})", x.ProductId.ToString())).ToListAsync();
        model.Locations = await _context.WarehouseLocations.OrderBy(x => x.LocationCode)
            .Select(x => new SelectListItem(x.LocationCode, x.LocationId.ToString())).ToListAsync();
        return model;
    }
}
