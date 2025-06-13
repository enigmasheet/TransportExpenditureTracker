// Controllers/ReportsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Helpers;
using TransportExpenditureTracker.Services;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IPartyService _partyService;
        private readonly IReportExportService _reportExportService;
        private readonly ApplicationDbContext _context;

        public ReportsController(IReportService reportService, IPartyService partyService, ApplicationDbContext context, IReportExportService reportExportService)
        {
            _reportService = reportService;
            _partyService = partyService;
            _context = context;
            _reportExportService = reportExportService;
        }
        private void LoadFiscalYearAndMonths()
        {
            ViewBag.FiscalYears = new SelectList(_context.FiscalYears.OrderByDescending(f => f.Name), "Name", "Name");

            var months = new[]
            {
        "Shrawan", "Bhadra", "Ashwin", "Kartik", "Mangsir", "Poush",
        "Magh", "Falgun", "Chaitra", "Baisakh", "Jestha", "Ashadh"
    };

            ViewBag.FiscalMonths = new SelectList(months);
        }



        private void LoadDropdowns(int? selectedPartyId = null, int? selectedItemId = null)
        {
            var parties = _context.Parties
                .Select(p => new
                {
                    p.PartyId,
                    DisplayText = p.VatNo + " - " + p.PartyName
                })
                .ToList();

            ViewData["PartyId"] = new SelectList(parties, "PartyId", "DisplayText", selectedPartyId);
            ViewData["ItemId"] = new SelectList(_context.Items, "ItemId", "ItemName", selectedItemId);
        }


        public async Task<IActionResult> VatInvoiceReport(ReportFilterViewModel filters)
        {
            LoadFiscalYearAndMonths();  
            LoadDropdowns(filters.PartyId, filters.ItemId);
            var companyId = UserClaimsHelper.GetCompanyId(User);

            var pagedResult = await _reportService.GetVatInvoiceReportAsync(filters);
            var parties = await _partyService.GetAllPartiesAsync(companyId.Value);

            var model = new ReportPageViewModel
            {
                Reports = pagedResult.Items,
                TotalRecords = pagedResult.TotalRecords,
                Filters = filters,
                Parties = parties
            };


            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> ExportVatInvoiceReport(string format, ReportFilterViewModel filters)
        {
            // Fetch all records for export
            filters.PageNumber = 1;
            filters.PageSize = int.MaxValue;

            var pagedResult = await _reportService.GetVatInvoiceReportAsync(filters);
            var report = pagedResult.Items;

            if (report == null || !report.Any())
            {
                TempData["Error"] = "No data available to export.";
                return RedirectToAction("VatInvoiceReport", filters);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var selectedCompany = await _context.UserCompanies
                .Include(u => u.Company)
                .Where(u => u.UserId == userId)
                .Select(u => u.Company)
                .FirstOrDefaultAsync();

            if (selectedCompany == null)
            {
                return BadRequest("No company selected.");
            }

            if (format?.ToLower() == "excel")
            {
                var excel = _reportExportService.GenerateVatInvoiceExcel(report, selectedCompany);
                return File(excel,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"VAT_Report_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            else // default to PDF
            {
                var pdf = _reportExportService.GenerateVatInvoiceReport(report, selectedCompany);
                return File(pdf, "application/pdf", $"VAT_Report_{DateTime.Now:yyyyMMdd}.pdf");
            }
        }

    }
}
