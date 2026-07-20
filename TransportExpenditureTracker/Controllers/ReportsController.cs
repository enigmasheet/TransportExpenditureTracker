using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Controllers;

[Authorize]
public class ReportsController(
    IReportService reportService,
    IReportExportService reportExportService,
    IExportJobService exportJobService,
    ApplicationDbContext ctx) : Controller
{
    private const string CtlDashboard = "Dashboard";
    private const string ReportsLabel = "Reports";
    private const string HomeLabel = "Home";
    private void LoadDropdowns(ReportFilterViewModel? filters = null)
    {
        var years = ctx.FiscalYears.OrderByDescending(f => f.Id).Select(f => f.Name).ToList();
        ViewData["FiscalYears"] = new SelectList(years, filters?.FiscalYear);

        DropdownHelper.LoadNepaliMonths(ViewData);
        if (!string.IsNullOrEmpty(filters?.NepaliMonth))
        {
            ViewData["NepaliMonths"] = new SelectList(
                NepaliDateHelper.NepaliMonthNames,
                filters.NepaliMonth);
        }

        DropdownHelper.LoadSuppliers(ctx, ViewData, filters?.SupplierId);
        DropdownHelper.LoadCategories(ctx, ViewData, filters?.CategoryId);
        DropdownHelper.LoadItems(ctx, ViewData, filters?.ItemId);
    }

    public async Task<IActionResult> Daily(ReportFilterViewModel filters)
    {
        if (!ModelState.IsValid) return BadRequest();
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ReportsLabel, null), ("Daily", null));
        LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await reportService.GetDailyReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> Monthly(ReportFilterViewModel filters)
    {
        if (!ModelState.IsValid) return BadRequest();
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ReportsLabel, null), ("Monthly", null));
        LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await reportService.GetMonthlyReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> FiscalYear(ReportFilterViewModel filters)
    {
        if (!ModelState.IsValid) return BadRequest();
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ReportsLabel, null), ("Fiscal Year", null));
        LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await reportService.GetFiscalYearReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> SupplierWise(ReportFilterViewModel filters)
    {
        if (!ModelState.IsValid) return BadRequest();
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ReportsLabel, null), ("Supplier-wise", null));
        LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await reportService.GetSupplierWiseReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> CategoryWise(ReportFilterViewModel filters)
    {
        if (!ModelState.IsValid) return BadRequest();
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ReportsLabel, null), ("Category-wise", null));
        LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await reportService.GetCategoryWiseReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> ItemWise(ReportFilterViewModel filters)
    {
        if (!ModelState.IsValid) return BadRequest();
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ReportsLabel, null), ("Item-wise", null));
        LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await reportService.GetItemWiseReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> VatPaid(ReportFilterViewModel filters)
    {
        if (!ModelState.IsValid) return BadRequest();
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ReportsLabel, null), ("VAT Paid", null));
        LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await reportService.GetVatPaidReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> PaymentMethod(ReportFilterViewModel filters)
    {
        if (!ModelState.IsValid) return BadRequest();
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ReportsLabel, null), ("Payment Method", null));
        LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await reportService.GetPaymentMethodReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> LocationWise(ReportFilterViewModel filters)
    {
        if (!ModelState.IsValid) return BadRequest();
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ReportsLabel, null), ("Location-wise", null));
        LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await reportService.GetLocationWiseReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> DetailedLedger(ReportFilterViewModel filters)
    {
        if (!ModelState.IsValid) return BadRequest();
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ReportsLabel, null), ("Detailed Ledger", null));
        LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await reportService.GetDetailedLedgerAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> Export(string format, string reportType, ReportFilterViewModel filters, string action)
    {
        if (!ModelState.IsValid) return BadRequest();

        if (action == "Download")
        {
            List<ReportRowViewModel> data = reportType switch
            {
                "Daily" => await reportService.GetDailyReportAsync(filters),
                "Monthly" => await reportService.GetMonthlyReportAsync(filters),
                "FiscalYear" => await reportService.GetFiscalYearReportAsync(filters),
                "SupplierWise" => await reportService.GetSupplierWiseReportAsync(filters),
                "CategoryWise" => await reportService.GetCategoryWiseReportAsync(filters),
                "ItemWise" => await reportService.GetItemWiseReportAsync(filters),
                "VatPaid" => await reportService.GetVatPaidReportAsync(filters),
                "PaymentMethod" => await reportService.GetPaymentMethodReportAsync(filters),
                "LocationWise" => await reportService.GetLocationWiseReportAsync(filters),
                "DetailedLedger" => await reportService.GetDetailedLedgerAsync(filters),
                _ => []
            };

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
            var filterJson = JsonSerializer.Serialize(filters);
            await exportJobService.EnqueueAsync(format, reportType, filterJson, email);
            TempData["Message"] = "Export job has been queued. You will receive an email once completed.";
            return RedirectToAction(reportType, filters);
        }
    }
}
