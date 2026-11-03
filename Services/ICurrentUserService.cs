namespace ChilliquerAspNetCore.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? FullName { get; }
    string? RoleName { get; }
    bool IsLoggedIn { get; }
    bool IsInRole(params string[] roles);
}
