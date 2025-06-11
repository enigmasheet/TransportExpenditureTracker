using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NepDate;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Helpers;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Controllers
{
    [Authorize]
    public class InvoicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentCompanyService _currentCompanyService;

        public InvoicesController(ApplicationDbContext context, ICurrentCompanyService currentCompanyService)
        {
            _context = context;
            _currentCompanyService = currentCompanyService;

        }
        private void LoadDropdowns(int? selectedPartyId = null, int? selectedItemId = null)
        {
            DropdownHelper.LoadFiscalYearAndMonths(_context, ViewData);
            DropdownHelper.LoadDropdowns(_context, ViewData, selectedPartyId, selectedItemId);
        }


        public async Task<IActionResult> Index()
        {
            var userCompanyId = UserClaimsHelper.GetCompanyId(User);
            var invoices = await _context.Invoices
                .Where(i => i.CompanyId == userCompanyId)
                .Include(i => i.Party)
                .Include(i => i.Item)
                .ToListAsync();

            return View(invoices);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Party)
                .Include(i => i.Item)
                .FirstOrDefaultAsync(m => m.InvoiceId == id);

            return invoice == null ? NotFound() : View(invoice);
        }

        [Authorize(Roles = "Admin,User")]
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Create([Bind("InvoiceId,InvoiceNo,Miti,PartyId,ItemId,Quantity,Rate,TaxableAmount,VatAmount,TotalInvoiceAmount,FiscalYear,FiscalMonth")] Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                invoice.CreatedAt = DateTime.Now;
                invoice.UpdatedAt = DateTime.Now;
                invoice.CompanyId= UserClaimsHelper.GetCompanyId(User);
                _context.Add(invoice);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(invoice.PartyId, invoice.ItemId);
            return View(invoice);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return NotFound();

            LoadDropdowns(invoice.PartyId, invoice.ItemId);
            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("InvoiceId,InvoiceNo,NepaliMiti,Miti,PartyId,ItemId,Quantity,Rate,TaxableAmount,VatAmount,TotalInvoiceAmount,FiscalYear,FiscalMonth")] Invoice updatedInvoice)
        {
            if (id != updatedInvoice.InvoiceId) return NotFound();

            if (!string.IsNullOrWhiteSpace(updatedInvoice.NepaliMiti))
            {
                var cleanedMiti = ConvertToEnglishDigits(updatedInvoice.NepaliMiti.Replace('-', '/'));
                updatedInvoice.NepaliMiti = cleanedMiti;
                updatedInvoice.Miti = new NepaliDate(cleanedMiti).EnglishDate;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingInvoice = await _context.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.InvoiceId == id);
                    if (existingInvoice == null) return NotFound();
                    updatedInvoice.CompanyId = existingInvoice.CompanyId;

                    updatedInvoice.CreatedAt = existingInvoice.CreatedAt;
                    updatedInvoice.UpdatedAt = DateTime.Now;
                    var userCompanyId = UserClaimsHelper.GetCompanyId(User);
                    var isAdmin = User.IsInRole("Admin");
                    if (!isAdmin && existingInvoice.CompanyId != userCompanyId)
                    {
                        return Forbid(); // or return Unauthorized();
                    }
                    _context.Update(updatedInvoice);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvoiceExists(updatedInvoice.InvoiceId)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(updatedInvoice.PartyId, updatedInvoice.ItemId);
            return View(updatedInvoice);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Party)
                .Include(i => i.Item)
                .FirstOrDefaultAsync(m => m.InvoiceId == id);

            return invoice == null ? NotFound() : View(invoice);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool InvoiceExists(int id)
        {
            return _context.Invoices.Any(e => e.InvoiceId == id);
        }

        [Authorize(Roles = "Admin,User")]
        public IActionResult CreateMultiple()
        {
            LoadDropdowns();

            var model = new InvoiceMonthly
            {
                Invoices = new List<Invoice> { new Invoice() }
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> CreateMultiple(InvoiceMonthly model)
        {
            if (model.Invoices == null || !model.Invoices.Any())
            {
                ModelState.AddModelError("", "Please add at least one invoice.");
                LoadDropdowns();
                return View("CreateMultiple", model);
            }

            if (ModelState.IsValid)
            {
                int companyId = (int)UserClaimsHelper.GetCompanyId(User); // ✅ get companyId once

                foreach (var invoice in model.Invoices)
                {
                    if (!string.IsNullOrWhiteSpace(invoice.NepaliMiti))
                    {
                        var cleanedMiti = ConvertToEnglishDigits(invoice.NepaliMiti.Replace('-', '/'));
                        invoice.NepaliMiti = cleanedMiti;
                        invoice.Miti = new NepaliDate(cleanedMiti).EnglishDate;
                    }

                    invoice.FiscalYear = model.FiscalYear;
                    invoice.FiscalMonth = model.FiscalMonth;
                    invoice.CreatedAt = DateTime.Now;
                    invoice.UpdatedAt = DateTime.Now;
                    invoice.CompanyId = companyId;
                }

                _context.Invoices.AddRange(model.Invoices);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns();
            return View("CreateMultiple", model);
        }

        public static string ConvertToEnglishDigits(string nepaliNumber)
        {
            if (string.IsNullOrEmpty(nepaliNumber)) return nepaliNumber;

            var map = new Dictionary<char, char>
            {
                ['०'] = '0',
                ['१'] = '1',
                ['२'] = '2',
                ['३'] = '3',
                ['४'] = '4',
                ['५'] = '5',
                ['६'] = '6',
                ['७'] = '7',
                ['८'] = '8',
                ['९'] = '9'
            };

            return new string(nepaliNumber.Select(c => map.TryGetValue(c, out var eng) ? eng : c).ToArray());
        }
    }
}
