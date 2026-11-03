using ChilliquerAspNetCore.Data;
using ChilliquerAspNetCore.Filters;
using ChilliquerAspNetCore.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChilliquerAspNetCore.Controllers;

[RequireRole("admin", "manager", "sales")]
public class CustomersController : Controller
{
    private readonly ChilliquerDbContext _context;
    public CustomersController(ChilliquerDbContext context) => _context = context;

    public async Task<IActionResult> Index(string? keyword)
    {
        var query = _context.Customers.Include(x => x.MemberCard).AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(x => x.FullName.Contains(keyword) || x.Cccd.Contains(keyword) || (x.Phone != null && x.Phone.Contains(keyword)));
        ViewBag.Keyword = keyword;
        return View(await query.OrderBy(x => x.FullName).ToListAsync());
    }

    public IActionResult Create() => View(new Customer { DateOfBirth = DateTime.Today.AddYears(-18) });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer customer)
    {
        if (!ModelState.IsValid) return View(customer);
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var customer = await _context.Customers.Include(x => x.MemberCard).FirstOrDefaultAsync(x => x.CustomerId == id);
        return customer == null ? NotFound() : View(customer);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        return View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Customer customer)
    {
        if (id != customer.CustomerId) return NotFound();
        if (!ModelState.IsValid) return View(customer);
        
        try
        {
            var existing = await _context.Customers.FindAsync(id);
            if (existing == null) return NotFound();
            
            existing.FullName = customer.FullName;
            existing.Cccd = customer.Cccd;
            existing.DateOfBirth = customer.DateOfBirth;
            existing.Gender = customer.Gender;
            existing.Address = customer.Address;
            existing.Phone = customer.Phone;
            existing.Email = customer.Email;
            existing.Occupation = customer.Occupation;
            existing.UpdatedAt = DateTime.Now;
            
            _context.Update(existing);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CustomerExists(customer.CustomerId)) return NotFound();
            else throw;
        }
        return RedirectToAction(nameof(Index));
    }

    private bool CustomerExists(Guid id) => _context.Customers.Any(e => e.CustomerId == id);
}
