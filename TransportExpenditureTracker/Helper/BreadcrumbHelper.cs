using Microsoft.AspNetCore.Mvc;

namespace TransportExpenditureTracker.Helper;

public static class BreadcrumbHelper
{
    public static void SetBreadcrumbs(this Controller controller, params (string Label, string? Url)[] crumbs)
    {
        controller.ViewData["Breadcrumbs"] = crumbs.ToList();
    }
}
