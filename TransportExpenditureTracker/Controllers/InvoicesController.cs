using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NepDate;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private void LoadFiscalYearAndMonths()
        {
            ViewBag.FiscalYears = new SelectList(_context.FiscalYears.OrderByDescending(f => f.Name), "Name", "Name");

            var months = new[] {
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

        // GET: Invoices
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Invoices
                .Include(i => i.Party)
                .Include(i => i.Item);

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Invoices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Party)
                .Include(i => i.Item)
                .FirstOrDefaultAsync(m => m.InvoiceId == id);

            if (invoice == null) return NotFound();

            return View(invoice);
        }

        // GET: Invoices/Create
        public IActionResult Create()
        {
            LoadDropdowns();
            LoadFiscalYearAndMonths();
            return View();
        }

        // POST: Invoices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InvoiceId,InvoiceNo,Miti,PartyId,ItemId,Quantity,Rate,TaxableAmount,VatAmount,TotalInvoiceAmount,FiscalYear,FiscalMonth")] Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                invoice.CreatedAt = DateTime.Now;
                invoice.UpdatedAt = DateTime.Now;

                _context.Add(invoice);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(invoice.PartyId, invoice.ItemId);
            LoadFiscalYearAndMonths();
            return View(invoice);
        }

        // GET: Invoices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return NotFound();

            LoadDropdowns(invoice.PartyId, invoice.ItemId);
            LoadFiscalYearAndMonths();

            return View(invoice);
        }

        // POST: Invoices/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
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

                    // Preserve created date, update updated date
                    updatedInvoice.CreatedAt = existingInvoice.CreatedAt;
                    updatedInvoice.UpdatedAt = DateTime.Now;

                    _context.Update(updatedInvoice);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvoiceExists(updatedInvoice.InvoiceId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(updatedInvoice.PartyId, updatedInvoice.ItemId);
            LoadFiscalYearAndMonths();
            return View(updatedInvoice);
        }

        // GET: Invoices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Party)
                .Include(i => i.Item)
                .FirstOrDefaultAsync(m => m.InvoiceId == id);

            if (invoice == null) return NotFound();

            return View(invoice);
        }

        // POST: Invoices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMultiple(InvoiceMonthly model)
        {
            if (model.Invoices == null || !model.Invoices.Any())
            {
                ModelState.AddModelError("", "Please add at least one invoice.");
                LoadDropdowns();
                LoadFiscalYearAndMonths();
                return View("CreateMultiple",model);
            }

        
            if (ModelState.IsValid)
            {
                foreach (var invoice in model.Invoices)
                {
                    var cleanedMiti = ConvertToEnglishDigits(invoice.NepaliMiti.Replace('-', '/'));
                    invoice.NepaliMiti = cleanedMiti;
                    invoice.Miti = new NepaliDate(cleanedMiti).EnglishDate;
                    invoice.FiscalYear = model.FiscalYear;
                    invoice.FiscalMonth = model.FiscalMonth;
                    invoice.CreatedAt = DateTime.Now;
                    invoice.UpdatedAt = DateTime.Now;
                }
                _context.Invoices.AddRange(model.Invoices);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
         

            LoadDropdowns();
            LoadFiscalYearAndMonths();
            return View("CreateMultiple", model);
        }
        public IActionResult CreateMultiple()
        {
            LoadDropdowns();
            LoadFiscalYearAndMonths();
            var model = new InvoiceMonthly
            {
                Invoices = new List<Invoice> { new Invoice() }
            };
            return View(model);
        }
        public static string ConvertToEnglishDigits(string nepaliNumber)
        {
            return nepaliNumber
                .Replace('०', '0')
                .Replace('१', '1')
                .Replace('२', '2')
                .Replace('३', '3')
                .Replace('४', '4')
                .Replace('५', '5')
                .Replace('६', '6')
                .Replace('७', '7')
                .Replace('८', '8')
                .Replace('९', '9');
        }

    }
}
