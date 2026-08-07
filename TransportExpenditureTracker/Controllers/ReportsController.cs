using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Controllers;

[Authorize]
public class ReportsController(
    IReportService reportService,
    IReportExportService reportExportService,
    IExportJobService exportJobService,
    IReportDataManager reportDataManager) : Controller
{
    private const string CtlDashboard = "Dashboard";
    private const string ReportsLabel = "Reports";
    private const string HomeLabel = "Home";

    private void ApplyUserScope(ReportFilterViewModel filters)
    {
        if (!User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            filters.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
    }

    private async Task LoadDropdowns(ReportFilterViewModel? filters = null)
    {
        var fiscalYears = await reportDataManager.GetFiscalYearsAsync();
        ViewData["FiscalYears"] = new SelectList(fiscalYears.Select(f => f.Name).ToList(), filters?.FiscalYear);

        DropdownHelper.LoadNepaliMonths(ViewData);
        if (!string.IsNullOrEmpty(filters?.NepaliMonth))
        {
            ViewData["NepaliMonths"] = new SelectList(
                NepaliDateHelper.NepaliMonthNames,
                filters.NepaliMonth);
        }

        var suppliers = await reportDataManager.GetSuppliersAsync();
        DropdownHelper.LoadSuppliers(suppliers, ViewData, filters?.SupplierId);
        var categories = await reportDataManager.GetCategoriesAsync();
        DropdownHelper.LoadCategories(categories, ViewData, filters?.CategoryId);
        var items = await reportDataManager.GetItemsAsync();
        DropdownHelper.LoadItems(items, ViewData, filters?.ItemId);
    }

    public async Task<IActionResult> Daily(ReportFilterViewModel filters)
    {
        if (!ModelState.IsValid) return BadRequest();
        ApplyUserScope(filters);
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ReportsLabel, null), ("Daily", null));
        await LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await reportService.GetDailyReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> Monthly(ReportFilterViewModel filters)
    {
        if (!ModelState.IsValid) return BadRequest();
        ApplyUserScope(filters);
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ReportsLabel, null), ("Monthly", null));
        await LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await reportService.GetMonthlyReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> FiscalYear(ReportFilterViewModel filters)
    {
        if (!ModelState.IsValid) return BadRequest();
        ApplyUserScope(filters);
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ReportsLabel, null), ("Fiscal Year", null));
        await LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await reportService.GetFiscalYearReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> SupplierWise(ReportFilterViewModel filters)
    {
        if (!ModelState.IsValid) return BadRequest();
        ApplyUserScope(filters);
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ReportsLabel, null), ("Supplier-wise", null));
        await LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await reportService.GetSupplierWiseReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> CategoryWise(ReportFilterViewModel filters)
    {
        if (!ModelState.IsValid) return BadRequest();
        ApplyUserScope(filters);
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ReportsLabel, null), ("Category-wise", null));
        await LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await reportService.GetCategoryWiseReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> ItemWise(ReportFilterViewModel filters)
    {
        if (!ModelState.IsValid) return BadRequest();
        ApplyUserScope(filters);
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ReportsLabel, null), ("Item-wise", null));
        await LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await reportService.GetItemWiseReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> VatPaid(ReportFilterViewModel filters)
    {
        if (!ModelState.IsValid) return BadRequest();
        ApplyUserScope(filters);
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ReportsLabel, null), ("VAT Paid", null));
        await LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await reportService.GetVatPaidReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> PaymentMethod(ReportFilterViewModel filters)
    {
        if (!ModelState.IsValid) return BadRequest();
        ApplyUserScope(filters);
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ReportsLabel, null), ("Payment Method", null));
        await LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await reportService.GetPaymentMethodReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> LocationWise(ReportFilterViewModel filters)
    {
        if (!ModelState.IsValid) return BadRequest();
        ApplyUserScope(filters);
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ReportsLabel, null), ("Location-wise", null));
        await LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await reportService.GetLocationWiseReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> DetailedLedger(ReportFilterViewModel filters)
    {
        if (!ModelState.IsValid) return BadRequest();
        ApplyUserScope(filters);
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ReportsLabel, null), ("Detailed Ledger", null));
        await LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await reportService.GetDetailedLedgerAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> Export(string format, string reportType, ReportFilterViewModel filters, string action)
    {
        if (!ModelState.IsValid) return BadRequest();
        ApplyUserScope(filters);

        if (action == "Download")
        {
            var data = await reportService.GetExportDataAsync(reportType, filters);

            byte[] fileBytes;
            var extension = format.ToLowerInvariant();

            string? contentType;
            if (extension == "pdf")
            {
                fileBytes = reportExportService.GeneratePdf(data, reportType);
                contentType = "application/pdf";
            }
            else if (extension == "csv")
            {
                fileBytes = reportExportService.GenerateCsv(data);
                contentType = "text/csv";
            }
            else
            {
                fileBytes = reportExportService.GenerateExcel(data, reportType);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                extension = "xlsx";
            }

            var fileName = $"{reportType}_{DateTime.Now:yyyyMMdd}.{extension}";
            return File(fileBytes, contentType, fileName);
        }
else
        {
            var email = User.Identity?.Name ?? "";
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var filterJson = JsonSerializer.Serialize(filters);
            await exportJobService.EnqueueAsync(format, reportType, filterJson, email, userId);
            TempData["Message"] = "Export job has been queued. You will receive an email once completed.";
            return RedirectToAction(reportType, filters);
        }
    }
}
