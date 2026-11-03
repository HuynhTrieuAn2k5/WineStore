using ChilliquerAspNetCore.Data;
using ChilliquerAspNetCore.Models.Entities;
using ChilliquerAspNetCore.Services;
using ChilliquerAspNetCore.ViewModels.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChilliquerAspNetCore.Controllers;

public class AuthController : Controller
{
    private readonly ChilliquerDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly IWebHostEnvironment _env;

    public AuthController(ChilliquerDbContext context, IPasswordService passwordService, IWebHostEnvironment env)
    {
        _context = context;
        _passwordService = passwordService;
        _env = env;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var user = await _context.UserAccounts
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Username == model.Username && x.IsActive);

            if (user == null)
            {
                await SafeAddAccessLog(null, "failed", "Không tìm thấy tài khoản");
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            if (!_passwordService.Verify(model.Password, user.PasswordHash))
            {
                await SafeAddAccessLog(null, "failed", "Sai mật khẩu");
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("RoleName", user.Role?.RoleName ?? string.Empty);

            await SafeAddAccessLog(user.UserId, "success", "Đăng nhập thành công");
            return Redirect(string.IsNullOrWhiteSpace(model.ReturnUrl) ? Url.Action("Index", "Dashboard")! : model.ReturnUrl);
        }
        catch (Exception ex)
        {
            // Show error details in development
            if (_env.IsDevelopment())
            {
                ModelState.AddModelError(string.Empty, $"Lỗi hệ thống: {ex.Message}");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi hệ thống. Vui lòng thử lại.");
            }
            return View(model);
        }
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();

    private async Task SafeAddAccessLog(Guid? userId, string status, string note)
    {
        try
        {
            _context.AccessLogs.Add(new AccessLog
            {
                UserId = userId,
                LoginAt = DateTime.Now,
                Status = status,
                Note = note,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            });
            await _context.SaveChangesAsync();
        }
        catch
        {
            // Don't let access log failures block login
        }
    }
}
