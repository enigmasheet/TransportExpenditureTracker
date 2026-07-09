using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    private void LoadDropdowns()
    {
        DropdownHelper.LoadFiscalYears(_ctx, ViewData);
        DropdownHelper.LoadNepaliMonths(ViewData);
        DropdownHelper.LoadSuppliers(_ctx, ViewData);
        DropdownHelper.LoadCategories(_ctx, ViewData);
        DropdownHelper.LoadItems(_ctx, ViewData);
    }

    public async Task<IActionResult> Daily(ReportFilterViewModel filters)
    {
        LoadDropdowns();
        var model = await _reportService.GetDailyReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> Monthly(ReportFilterViewModel filters)
    {
        LoadDropdowns();
        var model = await _reportService.GetMonthlyReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> FiscalYear(ReportFilterViewModel filters)
    {
        LoadDropdowns();
        var model = await _reportService.GetFiscalYearReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> SupplierWise(ReportFilterViewModel filters)
    {
        LoadDropdowns();
        var model = await _reportService.GetSupplierWiseReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> CategoryWise(ReportFilterViewModel filters)
    {
        LoadDropdowns();
        var model = await _reportService.GetCategoryWiseReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> ItemWise(ReportFilterViewModel filters)
    {
        LoadDropdowns();
        var model = await _reportService.GetItemWiseReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> VatPaid(ReportFilterViewModel filters)
    {
        LoadDropdowns();
        var model = await _reportService.GetVatPaidReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> PaymentMethod(ReportFilterViewModel filters)
    {
        LoadDropdowns();
        var model = await _reportService.GetPaymentMethodReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> LocationWise(ReportFilterViewModel filters)
    {
        LoadDropdowns();
        var model = await _reportService.GetLocationWiseReportAsync(filters);
        return View(model);
    }

    public async Task<IActionResult> DetailedLedger(ReportFilterViewModel filters)
    {
        LoadDropdowns();
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
