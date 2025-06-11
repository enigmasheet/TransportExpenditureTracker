using System.Security.Claims;

namespace TransportExpenditureTracker.Helpers
{
    public static class UserClaimsHelper
    {
        public static int? GetCompanyId(ClaimsPrincipal user)
        {
            var companyIdClaim = user.FindFirst("CompanyId")?.Value;
            return int.TryParse(companyIdClaim, out int companyId) ? companyId : null;
        }

        public static string? GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static string? GetUserEmail(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value;
        }

        public static string? GetUserName(ClaimsPrincipal user)
        {
            return user.Identity?.Name;
        }
    }
}
