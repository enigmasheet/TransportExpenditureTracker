using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;
    private readonly IReportExportService _reportExportService;
    private readonly IExportJobService _exportJobService;
    private readonly ApplicationDbContext _ctx;

    public ReportsController(
        IReportService reportService,
        IReportExportService reportExportService,
        IExportJobService exportJobService,
        ApplicationDbContext ctx)
    {
        _reportService = reportService;
        _reportExportService = reportExportService;
        _exportJobService = exportJobService;
        _ctx = ctx;
    }

    private void LoadDropdowns(ReportFilterViewModel? filters = null)
    {
        var years = _ctx.FiscalYears.OrderByDescending(f => f.Id).Select(f => f.Name).ToList();
        ViewData["FiscalYears"] = new SelectList(years, filters?.FiscalYear);

        DropdownHelper.LoadNepaliMonths(ViewData);
        if (!string.IsNullOrEmpty(filters?.NepaliMonth))
        {
            ViewData["NepaliMonths"] = new SelectList(
                new[] { "Baisakh(1)", "Jestha(2)", "Ashad(3)", "Shrawan(4)", "Bhadra(5)", "Ashwin(6)", "Kartik(7)", "Mangsir(8)", "Poush(9)", "Magh(10)", "Falgun(11)", "Chaitra(12)" },
                filters.NepaliMonth);
        }

        DropdownHelper.LoadSuppliers(_ctx, ViewData, filters?.SupplierId);
        DropdownHelper.LoadCategories(_ctx, ViewData, filters?.CategoryId);
        DropdownHelper.LoadItems(_ctx, ViewData, filters?.ItemId);
    }

    public async Task<IActionResult> Daily(ReportFilterViewModel filters)
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Reports", null), ("Daily", null));
        LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await _reportService.GetDailyReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> Monthly(ReportFilterViewModel filters)
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Reports", null), ("Monthly", null));
        LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await _reportService.GetMonthlyReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> FiscalYear(ReportFilterViewModel filters)
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Reports", null), ("Fiscal Year", null));
        LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await _reportService.GetFiscalYearReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> SupplierWise(ReportFilterViewModel filters)
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Reports", null), ("Supplier-wise", null));
        LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await _reportService.GetSupplierWiseReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> CategoryWise(ReportFilterViewModel filters)
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Reports", null), ("Category-wise", null));
        LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await _reportService.GetCategoryWiseReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> ItemWise(ReportFilterViewModel filters)
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Reports", null), ("Item-wise", null));
        LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await _reportService.GetItemWiseReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> VatPaid(ReportFilterViewModel filters)
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Reports", null), ("VAT Paid", null));
        LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await _reportService.GetVatPaidReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> PaymentMethod(ReportFilterViewModel filters)
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Reports", null), ("Payment Method", null));
        LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await _reportService.GetPaymentMethodReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> LocationWise(ReportFilterViewModel filters)
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Reports", null), ("Location-wise", null));
        LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await _reportService.GetLocationWiseReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> DetailedLedger(ReportFilterViewModel filters)
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Reports", null), ("Detailed Ledger", null));
        LoadDropdowns(filters);
        ViewBag.Filter = filters;
        var model = await _reportService.GetDetailedLedgerAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> Export(string format, string reportType, ReportFilterViewModel filters, string action)
    {
        if (action == "Download")
        {
            List<ReportRowViewModel> data = reportType switch
            {
                "Daily" => await _reportService.GetDailyReportAsync(filters),
                "Monthly" => await _reportService.GetMonthlyReportAsync(filters),
                "FiscalYear" => await _reportService.GetFiscalYearReportAsync(filters),
                "SupplierWise" => await _reportService.GetSupplierWiseReportAsync(filters),
                "CategoryWise" => await _reportService.GetCategoryWiseReportAsync(filters),
                "ItemWise" => await _reportService.GetItemWiseReportAsync(filters),
                "VatPaid" => await _reportService.GetVatPaidReportAsync(filters),
                "PaymentMethod" => await _reportService.GetPaymentMethodReportAsync(filters),
                "LocationWise" => await _reportService.GetLocationWiseReportAsync(filters),
                "DetailedLedger" => await _reportService.GetDetailedLedgerAsync(filters),
                _ => new List<ReportRowViewModel>()
            };

            byte[] fileBytes;
            var contentType = "application/octet-stream";
            var extension = format.ToLower();

            if (extension == "pdf")
            {
                fileBytes = _reportExportService.GeneratePdf(data, reportType);
                contentType = "application/pdf";
            }
            else if (extension == "csv")
            {
                fileBytes = _reportExportService.GenerateCsv(data);
                contentType = "text/csv";
            }
            else
            {
                fileBytes = _reportExportService.GenerateExcel(data, reportType);
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
            await _exportJobService.EnqueueAsync(format, reportType, filterJson, email);
            TempData["Message"] = "Export job has been queued. You will receive an email once completed.";
            return RedirectToAction(reportType, filters);
        }
    }
}
