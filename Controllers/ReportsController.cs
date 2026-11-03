using ChilliquerAspNetCore.Data;
using ChilliquerAspNetCore.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChilliquerAspNetCore.Controllers;

[RequireRole("admin", "manager")]
public class ReportsController : Controller
{
    private readonly ChilliquerDbContext _context;
    public ReportsController(ChilliquerDbContext context) => _context = context;

    public async Task<IActionResult> Index(int? year)
    {
        var reportYear = year ?? DateTime.Today.Year;
        ViewBag.Year = reportYear;
        ViewBag.TotalRevenue = await _context.Invoices
            .Where(x => x.Status == "completed" && x.SaleDate.Year == reportYear)
            .SumAsync(x => (decimal?)x.TotalAmount) ?? 0;
        ViewBag.InvoiceCount = await _context.Invoices.CountAsync(x => x.Status == "completed" && x.SaleDate.Year == reportYear);
        ViewBag.StockWarnings = await _context.Products.CountAsync(x => x.IsActive && x.StockQuantity <= x.MinStockAlert);

        var roleName = HttpContext.Session.GetString("RoleName");
        ViewBag.IsAdmin = string.Equals(roleName, "admin", StringComparison.OrdinalIgnoreCase);
        
        if (ViewBag.IsAdmin)
        {
            var monthlyRevenue = await _context.Invoices
                .Where(x => x.Status == "completed" && x.SaleDate.Year == reportYear)
                .GroupBy(x => x.SaleDate.Month)
                .Select(g => new { Month = g.Key, Total = g.Sum(x => (decimal?)x.TotalAmount) ?? 0 })
                .ToListAsync();

            var revenueData = new decimal[12];
            foreach (var item in monthlyRevenue)
            {
                revenueData[item.Month - 1] = item.Total;
            }
            ViewBag.MonthlyRevenue = System.Text.Json.JsonSerializer.Serialize(revenueData);
        }

        return View();
    }
}
