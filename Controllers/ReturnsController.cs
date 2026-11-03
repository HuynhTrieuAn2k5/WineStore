using ChilliquerAspNetCore.Data;
using ChilliquerAspNetCore.Filters;
using ChilliquerAspNetCore.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChilliquerAspNetCore.Controllers;

[RequireRole("admin", "manager", "sales")]
public class ReturnsController : Controller
{
    private readonly ChilliquerDbContext _context;
    public ReturnsController(ChilliquerDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var returns = await _context.Returns.Include(x => x.Invoice).Include(x => x.ProcessedByUser)
            .OrderByDescending(x => x.ReturnDate).ToListAsync();
        return View(returns);
    }

    public async Task<IActionResult> Create(Guid invoiceId)
    {
        var invoice = await _context.Invoices.Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.InvoiceId == invoiceId);
            
        if (invoice == null) 
            return NotFound("Hóa đơn không tồn tại.");

        var model = new ChilliquerAspNetCore.ViewModels.Returns.CreateReturnViewModel
        {
            InvoiceId = invoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            CustomerName = invoice.CustomerName,
            TotalAmount = invoice.TotalAmount,
            Items = invoice.Items.Select(x => new ChilliquerAspNetCore.ViewModels.Returns.ReturnItemViewModel
            {
                ItemId = x.ItemId,
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                UnitPrice = x.UnitPrice,
                MaxQuantity = x.Quantity,
                ReturnQuantity = 0,
                ConditionOk = true
            }).ToList()
        };
        
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ChilliquerAspNetCore.ViewModels.Returns.CreateReturnViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var validItems = model.Items.Where(x => x.ReturnQuantity > 0).ToList();
        if (!validItems.Any())
        {
            ModelState.AddModelError("", "Vui lòng chọn ít nhất một sản phẩm để trả.");
            return View(model);
        }

        if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var processedBy))
            return RedirectToAction("Login", "Auth");

        var totalRefund = 0m;
        var returnItems = new List<ReturnItem>();

        foreach(var item in validItems)
        {
            var subtotal = item.UnitPrice * item.ReturnQuantity;
            totalRefund += subtotal;
            returnItems.Add(new ReturnItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName ?? "",
                Quantity = item.ReturnQuantity,
                UnitPrice = item.UnitPrice,
                Subtotal = subtotal,
                ConditionOk = item.ConditionOk
            });
        }

        // Apply dummy cancellation fee if needed (e.g., 10%)
        var cancelFeePct = 10m;
        var cancelFeeAmt = Math.Round(totalRefund * cancelFeePct / 100, 0);
        var finalRefund = totalRefund - cancelFeeAmt;

        var ret = new Return
        {
            InvoiceId = model.InvoiceId,
            ProcessedBy = processedBy,
            ReturnDate = DateTime.Now,
            ReturnType = model.ReturnType,
            CancelFeePct = cancelFeePct,
            CancelFeeAmt = cancelFeeAmt,
            RefundAmount = finalRefund,
            Reason = model.Reason,
            Status = "approved",
            Items = returnItems
        };

        _context.Returns.Add(ret);
        
        var invoiceToUpdate = await _context.Invoices.FindAsync(model.InvoiceId);
        if(invoiceToUpdate != null) {
            invoiceToUpdate.Note = (invoiceToUpdate.Note + "\n[Đã đổi/trả một phần]").Trim();
            // Don't change status to completely 'returned' unless fully returned, for simplicity just append note
        }

        await _context.SaveChangesAsync();
        
        return RedirectToAction(nameof(Index));
    }
}
