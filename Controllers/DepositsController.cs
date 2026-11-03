using ChilliquerAspNetCore.Data;
using ChilliquerAspNetCore.Filters;
using ChilliquerAspNetCore.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChilliquerAspNetCore.Controllers;

[RequireRole("admin", "manager", "sales")]
public class DepositsController : Controller
{
    private readonly ChilliquerDbContext _context;
    public DepositsController(ChilliquerDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var deposits = await _context.Deposits.Include(x => x.Customer).Include(x => x.Invoice)
            .OrderByDescending(x => x.DepositDate).ToListAsync();
        return View(deposits);
    }

    public async Task<IActionResult> Create(Guid invoiceId)
    {
        var invoice = await _context.Invoices.Include(x => x.Card).ThenInclude(c => c!.Customer)
            .FirstOrDefaultAsync(x => x.InvoiceId == invoiceId);
            
        if (invoice == null) 
            return NotFound("Hóa đơn không tồn tại.");

        var deposit = new Deposit
        {
            InvoiceId = invoiceId,
            ExpectedPickup = DateTime.Today.AddDays(7),
            DepositAmount = invoice.TotalAmount / 2 // Default 50%
        };
        
        if (invoice.Card?.CustomerId != null)
        {
            deposit.CustomerId = invoice.Card.CustomerId;
            ViewBag.CustomerName = invoice.Card.Customer?.FullName;
        }
        else
        {
            ViewBag.Customers = await _context.Customers.OrderBy(c => c.FullName)
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(c.FullName, c.CustomerId.ToString())).ToListAsync();
            ViewBag.CustomerName = invoice.CustomerName;
        }
        
        ViewBag.InvoiceNumber = invoice.InvoiceNumber;
        ViewBag.TotalAmount = invoice.TotalAmount;
        
        return View(deposit);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Deposit deposit)
    {
        if (!ModelState.IsValid || deposit.CustomerId == Guid.Empty)
        {
            if (deposit.CustomerId == Guid.Empty) ModelState.AddModelError("CustomerId", "Vui lòng chọn khách hàng.");
            var invoice = await _context.Invoices.Include(x => x.Card).ThenInclude(c => c!.Customer)
                .FirstOrDefaultAsync(x => x.InvoiceId == deposit.InvoiceId);
            ViewBag.InvoiceNumber = invoice?.InvoiceNumber;
            if (invoice?.Card?.CustomerId != null)
            {
                ViewBag.CustomerName = invoice.Card.Customer?.FullName;
            }
            else
            {
                ViewBag.Customers = await _context.Customers.OrderBy(c => c.FullName)
                    .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(c.FullName, c.CustomerId.ToString())).ToListAsync();
            }
            ViewBag.TotalAmount = invoice?.TotalAmount;
            return View(deposit);
        }

        deposit.DepositDate = DateTime.Now;
        deposit.Status = "pending";
        _context.Deposits.Add(deposit);
        
        var invoiceToUpdate = await _context.Invoices.FindAsync(deposit.InvoiceId);
        if(invoiceToUpdate != null) {
            // Update invoice note to indicate deposit
            invoiceToUpdate.Note = (invoiceToUpdate.Note + "\n[Đã đặt cọc]").Trim();
        }

        await _context.SaveChangesAsync();
        
        return RedirectToAction(nameof(Index));
    }
}
