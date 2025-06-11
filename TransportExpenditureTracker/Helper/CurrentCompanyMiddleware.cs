using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Services.Interfaces;

namespace TransportExpenditureTracker.Helper
{
    public class CurrentCompanyMiddleware
    {
        private readonly RequestDelegate _next;

        public CurrentCompanyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext, ICurrentCompanyService companyService)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                dbContext.CurrentCompanyId = companyService.CompanyId;
            }

            await _next(context);
        }
    }
}
