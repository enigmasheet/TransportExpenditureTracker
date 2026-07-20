using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Services.Interfaces;

namespace TransportExpenditureTracker.Controllers;

[Authorize]
public class DashboardController(IDashboardService dashboardService, ApplicationDbContext ctx) : Controller
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
        var query = from h in ctx.ExpenseHeaders
                    from d in ctx.ExpenseDetails.Where(d => d.ExpenseId == h.ExpenseId)
                    where fiscalYear == null || h.FiscalYearNav.Name == fiscalYear
                    group d by h.NepaliMonth into g
                    select new { label = g.Key, value = g.Sum(d => d.TotalAmount) };

        var data = await query.ToListAsync();

        var sorted = data
            .Where(d => d.label != null)
            .Select(d => new
            {
                d.label,
                d.value,
                sort = d.label!.Contains('(') && d.label.EndsWith(')')
                    ? int.Parse(d.label.Split('(', ')', StringSplitOptions.None)[1], CultureInfo.InvariantCulture)
                    : 0
            })
            .OrderBy(d => d.sort)
            .Select(d => new { d.label, d.value })
            .ToList();

        return Json(sorted);
    }

    [HttpGet]
    public async Task<JsonResult> GetCategoryChartData(string? fiscalYear)
    {
        var query = from h in ctx.ExpenseHeaders
                    from d in ctx.ExpenseDetails.Where(d => d.ExpenseId == h.ExpenseId)
                    where fiscalYear == null || h.FiscalYearNav.Name == fiscalYear
                    group d by h.Category.CategoryName into g
                    select new { label = g.Key, value = g.Sum(d => d.TotalAmount) };

        var data = await query.ToListAsync();
        return Json(data.OrderByDescending(d => d.value).ToList());
    }

    [HttpGet]
    public async Task<JsonResult> GetSupplierChartData(string? fiscalYear)
    {
        var query = from h in ctx.ExpenseHeaders
                    from d in ctx.ExpenseDetails.Where(d => d.ExpenseId == h.ExpenseId)
                    where fiscalYear == null || h.FiscalYearNav.Name == fiscalYear
                    group d by h.Supplier.SupplierName into g
                    select new { label = g.Key, value = g.Sum(d => d.TotalAmount) };

        var data = await query.ToListAsync();
        return Json(data.OrderByDescending(d => d.value).Take(10).ToList());
    }

    [HttpGet]
    public async Task<JsonResult> GetVatChartData(string? fiscalYear)
    {
        var query = from h in ctx.ExpenseHeaders
                    from d in ctx.ExpenseDetails.Where(d => d.ExpenseId == h.ExpenseId)
                    where fiscalYear == null || h.FiscalYearNav.Name == fiscalYear
                    group d by h.NepaliMonth into g
                    select new { label = g.Key, value = g.Sum(d => d.VatAmount) };

        var data = await query.ToListAsync();

        var sorted = data
            .Where(d => d.label != null)
            .Select(d => new
            {
                d.label,
                d.value,
                sort = d.label!.Contains('(') && d.label.EndsWith(')')
                    ? int.Parse(d.label.Split('(', ')', StringSplitOptions.None)[1], CultureInfo.InvariantCulture)
                    : 0
            })
            .OrderBy(d => d.sort)
            .Select(d => new { d.label, d.value })
            .ToList();

        return Json(sorted);
    }

    [HttpGet]
    public async Task<JsonResult> GetFyComparisonData(string? fiscalYear)
    {
        var query = from h in ctx.ExpenseHeaders
                    from d in ctx.ExpenseDetails.Where(d => d.ExpenseId == h.ExpenseId)
                    where fiscalYear == null || h.FiscalYearNav.Name == fiscalYear
                    group d by h.FiscalYearNav.Name into g
                    select new { label = g.Key, value = g.Sum(d => d.TotalAmount) };

        var data = await query.ToListAsync();
        return Json(data.Where(d => d.label != null).OrderBy(d => d.label).ToList());
    }
}