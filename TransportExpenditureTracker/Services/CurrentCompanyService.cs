using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TransportExpenditureTracker.Services.Interfaces;

namespace TransportExpenditureTracker.Services
{
    public class CurrentCompanyService : ICurrentCompanyService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentCompanyService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int CompanyId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;

                if (user == null || !user.Identity.IsAuthenticated)
                    return 0; // or throw exception, or return a default value

                var claim = user.FindFirst("CompanyId");
                if (claim != null && int.TryParse(claim.Value, out int companyId))
                {
                    return companyId;
                }

                return 0; // or handle missing claim as needed
            }
        }
    }
}
