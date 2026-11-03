using ChilliquerAspNetCore.Data;
using ChilliquerAspNetCore.Filters;
using ChilliquerAspNetCore.Models.Entities;
using ChilliquerAspNetCore.ViewModels.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ChilliquerAspNetCore.Controllers;

public class ProductsController : Controller
{
    private readonly ChilliquerDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ProductsController(ChilliquerDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<IActionResult> Index(string? keyword, int? categoryId)
    {
        var query = _context.Products
            .Include(x => x.Category)
            .Include(x => x.Manufacturer)
            .Where(x => x.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(x => x.ProductCode.Contains(keyword) || x.Name.Contains(keyword));

        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId.Value);

        ViewBag.Keyword = keyword;
        ViewBag.CategoryId = categoryId;
        ViewBag.Categories = await _context.ProductCategories.OrderBy(c => c.CategoryName).ToListAsync();
        
        return View(await query.OrderBy(x => x.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var product = await _context.Products
            .Include(x => x.Category)
            .Include(x => x.Manufacturer)
            .Include(x => x.Supplier)
            .FirstOrDefaultAsync(x => x.ProductId == id);
        return product == null ? NotFound() : View(product);
    }

    [RequireRole("admin", "manager")]
    public async Task<IActionResult> Create()
    {
        var model = await BuildFormModel();
        model.ProductCode = await GenerateNextProductCode();
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequireRole("admin", "manager")]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        // Auto-generate ProductCode if empty or keep the pre-filled one
        if (string.IsNullOrWhiteSpace(model.ProductCode))
        {
            model.ProductCode = await GenerateNextProductCode();
        }

        // Check for duplicate ProductCode
        if (await _context.Products.AnyAsync(x => x.ProductCode == model.ProductCode.Trim()))
        {
            model.ProductCode = await GenerateNextProductCode();
        }

        ModelState.Remove(nameof(model.ProductCode));
        if (!ModelState.IsValid) return View(await BuildFormModel(model));

        string? imageUrl = model.ImageUrl;
        if (model.ImageUpload != null && model.ImageUpload.Length > 0)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.ImageUpload.CopyToAsync(fileStream);
            }
            imageUrl = "/images/products/" + uniqueFileName;
        }

        _context.Products.Add(new Product
        {
            ProductCode = model.ProductCode.Trim(),
            Name = model.Name.Trim(),
            CategoryId = model.CategoryId,
            ManufacturerId = model.ManufacturerId,
            SupplierId = model.SupplierId,
            Origin = model.Origin,
            VolumeMl = model.VolumeMl,
            AlcoholPct = model.AlcoholPct,
            SellPrice = model.SellPrice,
            MinStockAlert = model.MinStockAlert,
            ImageUrl = imageUrl,
            IsActive = true
        });
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [RequireRole("admin", "manager")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        return View(await BuildFormModel(new ProductFormViewModel
        {
            ProductId = product.ProductId,
            ProductCode = product.ProductCode,
            Name = product.Name,
            CategoryId = product.CategoryId,
            ManufacturerId = product.ManufacturerId,
            SupplierId = product.SupplierId,
            Origin = product.Origin,
            VolumeMl = product.VolumeMl,
            AlcoholPct = product.AlcoholPct,
            SellPrice = product.SellPrice,
            MinStockAlert = product.MinStockAlert,
            ImageUrl = product.ImageUrl
        }));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequireRole("admin", "manager")]
    public async Task<IActionResult> Edit(Guid id, ProductFormViewModel model)
    {
        if (!ModelState.IsValid) return View(await BuildFormModel(model));

        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        string? imageUrl = model.ImageUrl;
        if (model.ImageUpload != null && model.ImageUpload.Length > 0)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.ImageUpload.CopyToAsync(fileStream);
            }
            imageUrl = "/images/products/" + uniqueFileName;
        }

        product.ProductCode = model.ProductCode.Trim();
        product.Name = model.Name.Trim();
        product.CategoryId = model.CategoryId;
        product.ManufacturerId = model.ManufacturerId;
        product.SupplierId = model.SupplierId;
        product.Origin = model.Origin;
        product.VolumeMl = model.VolumeMl;
        product.AlcoholPct = model.AlcoholPct;
        product.SellPrice = model.SellPrice;
        product.MinStockAlert = model.MinStockAlert;
        product.ImageUrl = imageUrl;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<ProductFormViewModel> BuildFormModel(ProductFormViewModel? model = null)
    {
        model ??= new ProductFormViewModel();
        model.Categories = await _context.ProductCategories.OrderBy(x => x.CategoryName)
            .Select(x => new SelectListItem(x.CategoryName, x.CategoryId.ToString())).ToListAsync();
        model.Manufacturers = await _context.Manufacturers.Where(x => x.IsActive).OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.ManufacturerId.ToString())).ToListAsync();
        model.Suppliers = await _context.Suppliers.Where(x => x.IsActive).OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.SupplierId.ToString())).ToListAsync();
        return model;
    }

    private async Task<string> GenerateNextProductCode()
    {
        // Get all ProductCodes matching pattern SPxxx
        var maxCode = await _context.Products
            .Where(x => x.ProductCode.StartsWith("SP"))
            .OrderByDescending(x => x.ProductCode)
            .Select(x => x.ProductCode)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (maxCode != null && int.TryParse(maxCode.Substring(2), out var currentMax))
        {
            nextNumber = currentMax + 1;
        }

        return $"SP{nextNumber:D3}";
    }

    [RequireLogin]
    public async Task<IActionResult> Buy(Guid id)
    {
        var product = await _context.Products
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.ProductId == id && x.IsActive);
        if (product == null) return NotFound();
        return RedirectToAction("Create", "Invoices", new { productId = id });
    }
}
