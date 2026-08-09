using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TransportExpenditureTracker.Services.Interfaces;

namespace TransportExpenditureTracker.Services;

public sealed class RequireCompanySetupFilter(ICompanyService companyService) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;
        if (user.Identity is { IsAuthenticated: true })
        {
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            if (controller != "Company" || action is not ("Setup" or "Index"))
            {
                if (!await companyService.IsSetupCompleteAsync())
                {
                    context.Result = new RedirectToActionResult("Setup", "Company", null);
                    return;
                }
            }
        }

        await next();
    }
}