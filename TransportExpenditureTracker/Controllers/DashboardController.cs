using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Services.Interfaces;

namespace TransportExpenditureTracker.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly ApplicationDbContext _ctx;

    public DashboardController(IDashboardService dashboardService, ApplicationDbContext ctx)
    {
        _dashboardService = dashboardService;
        _ctx = ctx;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _dashboardService.GetDashboardAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<JsonResult> GetMonthlyChartData()
    {
        var details = await _ctx.ExpenseDetails.Include(d => d.Expense).ToListAsync();
        var data = details
            .GroupBy(d => d.Expense.NepaliMonth ?? "")
            .Select(g => new { month = g.Key, amount = g.Sum(d => d.TotalAmount) })
            .ToList();
        return Json(data);
    }

    [HttpGet]
    public async Task<JsonResult> GetCategoryChartData()
    {
        var details = await _ctx.ExpenseDetails.Include(d => d.Expense).ThenInclude(e => e.Category).ToListAsync();
        var data = details
            .GroupBy(d => d.Expense.Category.CategoryName)
            .Select(g => new { category = g.Key, amount = g.Sum(d => d.TotalAmount) })
            .ToList();
        return Json(data);
    }

    [HttpGet]
    public async Task<JsonResult> GetSupplierChartData()
    {
        var details = await _ctx.ExpenseDetails.Include(d => d.Expense).ThenInclude(e => e.Supplier).ToListAsync();
        var data = details
            .GroupBy(d => d.Expense.Supplier.SupplierName)
            .Select(g => new { supplier = g.Key, amount = g.Sum(d => d.TotalAmount) })
            .ToList();
        return Json(data);
    }

    [HttpGet]
    public async Task<JsonResult> GetVatChartData()
    {
        var details = await _ctx.ExpenseDetails.Include(d => d.Expense).ToListAsync();
        var data = details
            .GroupBy(d => d.Expense.NepaliMonth ?? "")
            .Select(g => new { month = g.Key, vatAmount = g.Sum(d => d.VatAmount) })
            .ToList();
        return Json(data);
    }

    [HttpGet]
    public async Task<JsonResult> GetFyComparisonData()
    {
        var details = await _ctx.ExpenseDetails.Include(d => d.Expense).ToListAsync();
        var data = details
            .GroupBy(d => d.Expense.FiscalYear ?? "")
            .Select(g => new { fy = g.Key, amount = g.Sum(d => d.TotalAmount) })
            .ToList();
        return Json(data);
    }
}
