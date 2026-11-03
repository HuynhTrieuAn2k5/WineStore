using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ChilliquerAspNetCore.Filters;

public class RequireRoleAttribute : RequireLoginAttribute
{
    private readonly string[] _roles;
    public RequireRoleAttribute(params string[] roles) => _roles = roles;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
        if (context.Result != null) return;

        var role = context.HttpContext.Session.GetString("RoleName");
        if (role == null || !_roles.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
        }
    }
}
