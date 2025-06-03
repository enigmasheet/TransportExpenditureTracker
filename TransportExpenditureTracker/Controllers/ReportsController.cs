// Controllers/ReportsController.cs
using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Services;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IPartyService _partyService;
        private readonly ApplicationDbContext _context;

        public ReportsController(IReportService reportService, IPartyService partyService, ApplicationDbContext context)
        {
            _reportService = reportService;
            _partyService = partyService;
            _context = context;


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

            var report = await _reportService.GetVatInvoiceReportAsync(filters);
            var parties = await _partyService.GetAllPartiesAsync();

            var model = new ReportPageViewModel
            {
                Reports = report,
                Filters = filters,
                Parties = parties
            };

            return View(model);
        }



    }
}
