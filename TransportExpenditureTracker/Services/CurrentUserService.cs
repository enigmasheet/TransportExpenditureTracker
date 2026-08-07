using System.Security.Claims;

namespace TransportExpenditureTracker.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : Interfaces.ICurrentUserService
{
    public string? UserId => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public bool IsAdmin =>
        httpContextAccessor.HttpContext?.User.IsInRole("Admin") == true ||
        httpContextAccessor.HttpContext?.User.IsInRole("SuperAdmin") == true;
}