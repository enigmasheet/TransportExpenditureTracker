using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Services.Interfaces;

namespace TransportExpenditureTracker.Controllers;

[Authorize]
public class DashboardController(IDashboardService dashboardService, IDashboardDataManager dashboardDataManager) : Controller
{

    public async Task<IActionResult> Index(string? fiscalYear)
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Dashboard", null));
        var model = await dashboardService.GetDashboardAsync(fiscalYear);
        return View(model);
    }

    [HttpGet]
    public async Task<JsonResult> GetMonthlyChartData(string? fiscalYear)
    {
        var data = await dashboardDataManager.GetMonthlyChartDataAsync(fiscalYear);
        return Json(data.Select(d => new { label = d.Label, value = d.Value }).ToList());
    }

    [HttpGet]
    public async Task<JsonResult> GetCategoryChartData(string? fiscalYear)
    {
        var data = await dashboardDataManager.GetCategoryChartDataAsync(fiscalYear);
        return Json(data.Select(d => new { label = d.Label, value = d.Value }).ToList());
    }

    [HttpGet]
    public async Task<JsonResult> GetSupplierChartData(string? fiscalYear)
    {
        var data = await dashboardDataManager.GetSupplierChartDataAsync(fiscalYear);
        return Json(data.Select(d => new { label = d.Label, value = d.Value }).ToList());
    }

    [HttpGet]
    public async Task<JsonResult> GetVatChartData(string? fiscalYear)
    {
        var data = await dashboardDataManager.GetVatChartDataAsync(fiscalYear);
        return Json(data.Select(d => new { label = d.Label, value = d.Value }).ToList());
    }

    [HttpGet]
    public async Task<JsonResult> GetFyComparisonData(string? fiscalYear)
    {
        var data = await dashboardDataManager.GetFyComparisonDataAsync(fiscalYear);
        return Json(data.Select(d => new { label = d.Label, value = d.Value }).ToList());
    }
}
