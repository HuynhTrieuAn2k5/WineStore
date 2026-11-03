namespace ChilliquerAspNetCore.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public CurrentUserService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    private ISession? Session => _httpContextAccessor.HttpContext?.Session;

    public Guid? UserId
    {
        get
        {
            var value = Session?.GetString("UserId");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? FullName => Session?.GetString("FullName");
    public string? RoleName => Session?.GetString("RoleName");
    public bool IsLoggedIn => UserId.HasValue;
    public bool IsInRole(params string[] roles) => RoleName != null && roles.Contains(RoleName, StringComparer.OrdinalIgnoreCase);
}
