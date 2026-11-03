using ChilliquerAspNetCore.Data;
using ChilliquerAspNetCore.Filters;
using ChilliquerAspNetCore.Models.Entities;
using ChilliquerAspNetCore.ViewModels.Invoices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ChilliquerAspNetCore.Controllers;

[RequireRole("admin", "manager", "sales")]
public class InvoicesController : Controller
{
    private readonly ChilliquerDbContext _context;
    public InvoicesController(ChilliquerDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var invoices = await _context.Invoices.Include(x => x.Cashier).Include(x => x.Card)
            .OrderByDescending(x => x.SaleDate).ToListAsync();
        return View(invoices);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var invoice = await _context.Invoices.Include(x => x.Cashier).Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.InvoiceId == id);
        return invoice == null ? NotFound() : View(invoice);
    }

    public async Task<IActionResult> Create() => View(await BuildCreateModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateInvoiceViewModel model)
    {
        if (!ModelState.IsValid || model.Items == null || !model.Items.Any())
        {
            if (model.Items == null || !model.Items.Any()) ModelState.AddModelError("", "Vui lòng chọn ít nhất một sản phẩm.");
            return View(await BuildCreateModel(model));
        }

        var discountPct = 0m;
        if (model.CardId.HasValue)
        {
            var card = await _context.MemberCards.FirstOrDefaultAsync(x => x.CardId == model.CardId && x.Status == "active" && x.ExpiryDate >= DateTime.Today);
            if (card != null)
            {
                var policy = await _context.DiscountPolicies.FirstOrDefaultAsync(x => x.CardType == card.CardType && x.IsActive);
                discountPct = policy?.DiscountPct ?? 0;
            }
        }

        var totalBeforeDiscount = 0m;
        var invoiceItems = new List<InvoiceItem>();

        foreach (var item in model.Items)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == item.ProductId && x.IsActive);
            if (product == null)
            {
                ModelState.AddModelError("", "Một trong số sản phẩm không tồn tại.");
                return View(await BuildCreateModel(model));
            }
            if (product.StockQuantity < item.Quantity)
            {
                ModelState.AddModelError("", $"Sản phẩm {product.Name} không đủ tồn kho (còn {product.StockQuantity}).");
                return View(await BuildCreateModel(model));
            }

            var subtotal = product.SellPrice * item.Quantity;
            totalBeforeDiscount += subtotal;

            invoiceItems.Add(new InvoiceItem
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                UnitPrice = product.SellPrice,
                Quantity = item.Quantity,
                Subtotal = subtotal
            });
            
            // Stock deduction is handled by SQL trigger trg_invoice_items_decrease_stock
        }

        var discountAmount = Math.Round(totalBeforeDiscount * discountPct / 100, 0);
        var totalAmount = totalBeforeDiscount - discountAmount;

        if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var cashierId))
            return RedirectToAction("Login", "Auth");

        var invoice = new Invoice
        {
            CashierId = cashierId,
            CardId = model.CardId,
            CustomerName = model.CustomerName,
            SaleDate = DateTime.Now,
            SaleType = "direct",
            TotalBeforeDiscount = totalBeforeDiscount,
            DiscountPct = discountPct,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            PaymentMethod = model.PaymentMethod,
            Status = "completed",
            Note = model.Note,
            Items = invoiceItems
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = invoice.InvoiceId });
    }

    private async Task<CreateInvoiceViewModel> BuildCreateModel(CreateInvoiceViewModel? model = null)
    {
        model ??= new CreateInvoiceViewModel();
        model.Products = await _context.Products.Where(x => x.IsActive && x.StockQuantity > 0).OrderBy(x => x.Name)
            .Select(x => new SelectListItem($"{x.ProductCode} - {x.Name} ({x.StockQuantity})", x.ProductId.ToString())).ToListAsync();
            
        var policies = await _context.DiscountPolicies.Where(x => x.IsActive).ToDictionaryAsync(x => x.CardType, x => x.DiscountPct);

        var cards = await _context.MemberCards.Include(x => x.Customer)
            .Where(x => x.Status == "active" && x.ExpiryDate >= DateTime.Today)
            .OrderBy(x => x.CardNumber)
            .ToListAsync();
            
        model.MemberCards = cards.Select(x => {
            var discount = policies.TryGetValue(x.CardType, out var pct) ? pct : 0;
            var text = $"{x.CardNumber} - {x.Customer?.FullName}";
            if (discount > 0) text += $" (Giảm {discount}%)";
            return new SelectListItem(text, x.CardId.ToString());
        }).ToList();
        
        return model;
    }
}
