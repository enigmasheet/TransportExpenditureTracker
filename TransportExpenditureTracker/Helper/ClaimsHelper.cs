using System.Security.Claims;

namespace TransportExpenditureTracker.Helper;

public static class ClaimsHelper
{
    public static string GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }

    public static string GetUserEmail(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
    }
}
