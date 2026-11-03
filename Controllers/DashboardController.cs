using ChilliquerAspNetCore.Data;
using ChilliquerAspNetCore.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChilliquerAspNetCore.Controllers;

[RequireLogin]
public class DashboardController : Controller
{
    private readonly ChilliquerDbContext _context;
    public DashboardController(ChilliquerDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        ViewBag.ProductCount = await _context.Products.CountAsync(x => x.IsActive);
        ViewBag.CustomerCount = await _context.Customers.CountAsync();
        ViewBag.InvoiceCount = await _context.Invoices.CountAsync();
        ViewBag.StockWarningCount = await _context.Products.CountAsync(x => x.IsActive && x.StockQuantity <= x.MinStockAlert);
        ViewBag.TodayRevenue = await _context.Invoices
            .Where(x => x.Status == "completed" && x.SaleDate.Date == DateTime.Today)
            .SumAsync(x => (decimal?)x.TotalAmount) ?? 0;

        // Get top stock warning products for dashboard display
        ViewBag.StockWarningProducts = await _context.Products
            .Include(x => x.Category)
            .Where(x => x.IsActive && x.StockQuantity <= x.MinStockAlert)
            .OrderBy(x => x.StockQuantity)
            .Take(5)
            .ToListAsync();

        return View();
    }
}
