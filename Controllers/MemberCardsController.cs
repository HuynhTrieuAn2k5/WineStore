using ChilliquerAspNetCore.Data;
using ChilliquerAspNetCore.Filters;
using ChilliquerAspNetCore.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ChilliquerAspNetCore.Controllers;

[RequireRole("admin", "manager", "sales")]
public class MemberCardsController : Controller
{
    private readonly ChilliquerDbContext _context;
    public MemberCardsController(ChilliquerDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var cards = await _context.MemberCards.Include(x => x.Customer).OrderBy(x => x.CardNumber).ToListAsync();
        return View(cards);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Customers = await _context.Customers.Where(c => c.MemberCard == null)
            .OrderBy(c => c.FullName)
            .Select(c => new SelectListItem(c.FullName, c.CustomerId.ToString())).ToListAsync();
        return View(new MemberCard { IssueDate = DateTime.Today, ExpiryDate = DateTime.Today.AddYears(1), FeePaid = 20000 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MemberCard card)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Customers = await _context.Customers.Where(c => c.MemberCard == null)
                .OrderBy(c => c.FullName)
                .Select(c => new SelectListItem(c.FullName, c.CustomerId.ToString())).ToListAsync();
            return View(card);
        }
        var userIdText = HttpContext.Session.GetString("UserId");
        if (Guid.TryParse(userIdText, out var userId)) card.IssuedBy = userId;
        card.CardNumber = null; // Trigger SQL Server sẽ tự sinh TV000xxxxx
        card.Status = "active";
        _context.MemberCards.Add(card);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var card = await _context.MemberCards.Include(c => c.Customer).FirstOrDefaultAsync(c => c.CardId == id);
        if (card == null) return NotFound();
        return View(card);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, MemberCard card)
    {
        if (id != card.CardId) return NotFound();
        
        // Remove Customer validation as we only edit the card's fields
        ModelState.Remove("Customer");
        
        if (!ModelState.IsValid) return View(card);
        
        try
        {
            var existing = await _context.MemberCards.FindAsync(id);
            if (existing == null) return NotFound();
            
            existing.CardType = card.CardType;
            existing.ExpiryDate = card.ExpiryDate;
            existing.Status = card.Status;
            existing.UpdatedAt = DateTime.Now;
            
            _context.Update(existing);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MemberCardExists(card.CardId)) return NotFound();
            else throw;
        }
        return RedirectToAction(nameof(Index));
    }

    private bool MemberCardExists(Guid id) => _context.MemberCards.Any(e => e.CardId == id);

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var memberCard = await _context.MemberCards.FindAsync(id);
        if (memberCard != null)
        {
            _context.MemberCards.Remove(memberCard);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
