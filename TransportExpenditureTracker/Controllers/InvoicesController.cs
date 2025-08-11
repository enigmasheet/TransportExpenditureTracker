using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
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
                int? companyIdNullable = UserClaimsHelper.GetCompanyId(User);
                if (companyIdNullable == null)
                {
                    return BadRequest("Company ID is required.");
                }
                int companyId = companyIdNullable.Value; // Safely access the value
                invoice.CompanyId = companyId;
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
                ProcessNepaliDate(updatedInvoice);
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
                int? companyIdNullable = UserClaimsHelper.GetCompanyId(User);
                if (companyIdNullable == null)
                {
                    return BadRequest("Company ID is required.");
                }
                const int batchSize = 100;
                var invoices = model.Invoices;
                int companyId = companyIdNullable.Value;

                _context.ChangeTracker.AutoDetectChangesEnabled = false; // improve performance
                try
                {
                    for (int i = 0; i < invoices.Count; i += batchSize)
                    {
                        var batch = invoices.Skip(i).Take(batchSize).ToList();

                        foreach (var invoice in batch)
                        {
                            if (!string.IsNullOrWhiteSpace(invoice.NepaliMiti))
                            {
                                ProcessNepaliDate(invoice);
                            }

                            invoice.FiscalYear = model.FiscalYear;
                            invoice.FiscalMonth = model.FiscalMonth;
                            invoice.CreatedAt = DateTime.Now;
                            invoice.UpdatedAt = DateTime.Now;
                            invoice.CompanyId = companyId;
                        }

                        _context.Invoices.AddRange(batch);
                        await _context.SaveChangesAsync();
                        _context.ChangeTracker.Clear(); // clear tracked entities after save
                    }
                }
                finally
                {
                    _context.ChangeTracker.AutoDetectChangesEnabled = true; // restore tracking
                }

                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns();
            return View("CreateMultiple", model);
        }

        [Authorize(Roles = "Admin,User")]
        public IActionResult ImportExcel()
        {
            LoadDropdowns();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> ImportExcel(string FiscalYear, string FiscalMonth, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a valid Excel file.");
                LoadDropdowns();
                return View();
            }

            var model = new InvoiceMonthly
            {
                Invoices = new List<Invoice>()
            };

            var importErrors = new List<string>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                    int rowIndex = 2; // starting row after header

                    
                        foreach (var row in rows)
                        {
                            var invoiceNoCell = row.Cell(3).GetString().Trim();
                            var vatNoCell = row.Cell(6).GetString().Trim();

                            if (string.IsNullOrWhiteSpace(invoiceNoCell) && string.IsNullOrWhiteSpace(vatNoCell))
                            {
                                rowIndex++;
                                continue;
                            }

                            var nepaliMiti = row.Cell(2).GetString();

                            if (string.IsNullOrWhiteSpace(nepaliMiti))
                            {
                                importErrors.Add($"Row {rowIndex}: Nepali date is empty");
                                rowIndex++;
                                continue;
                            }

                            var vatNo = vatNoCell;

                            if (string.IsNullOrWhiteSpace(vatNo) || vatNo.Length != 9)
                            {
                                importErrors.Add($"Row {rowIndex}: Invalid VAT No '{vatNo}'");
                                rowIndex++;
                                continue;
                            }

                            var party = await _context.Parties.FirstOrDefaultAsync(p => p.VatNo == vatNo);
                            if (party == null)
                            {
                                importErrors.Add($"Row {rowIndex}: VAT No '{vatNo}' not found in system");
                                rowIndex++;
                                continue;
                            }

                            var invoiceNo = invoiceNoCell;
                            var companyId = UserClaimsHelper.GetCompanyId(User) ?? 0;

                            bool exists = await _context.Invoices.AnyAsync(i => i.InvoiceNo == invoiceNo && i.CompanyId == companyId);
                            if (exists)
                            {
                                importErrors.Add($"Row {rowIndex}: Invoice No '{invoiceNo}' already exists");
                                rowIndex++;
                                continue;
                            }

                            // Parse numbers safely
                            if (!TryGetDecimalFromCell(row.Cell(8), out decimal quantity))
                            {
                                importErrors.Add($"Row {rowIndex}: Invalid Quantity");
                                rowIndex++;
                                continue;
                            }

                            if (!TryGetDecimalFromCell(row.Cell(9), out decimal rate))
                            {
                                importErrors.Add($"Row {rowIndex}: Invalid Rate");
                                rowIndex++;
                                continue;
                            }

                            if (!TryGetDecimalFromCell(row.Cell(10), out decimal taxableAmount))
                            {
                                importErrors.Add($"Row {rowIndex}: Invalid Taxable Amount");
                                rowIndex++;
                                continue;
                            }

                            if (!TryGetDecimalFromCell(row.Cell(11), out decimal vatAmount))
                            {
                                importErrors.Add($"Row {rowIndex}: Invalid VAT Amount");
                                rowIndex++;
                                continue;
                            }

                            if (!TryGetDecimalFromCell(row.Cell(12), out decimal totalInvoiceAmount))
                            {
                                importErrors.Add($"Row {rowIndex}: Invalid Total Invoice Amount");
                                rowIndex++;
                                continue;
                            }

                            var invoice = new Invoice
                            {
                                NepaliMiti = nepaliMiti,
                                InvoiceNo = invoiceNo,
                                PartyId = party.PartyId,
                                ItemId = GetItemIdByName(row.Cell(7).GetString()),
                                Quantity = quantity,
                                Rate = rate,
                                TaxableAmount = quantity*rate,
                                VatAmount = (quantity * rate) * 0.13m,  // 13% VAT as decimal
                                TotalInvoiceAmount = (quantity * rate) * 1.13m,  // Total = taxable + VAT
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now,
                                CompanyId = companyId,
                                FiscalYear = FiscalYear,
                                FiscalMonth = FiscalMonth
                            };

                            ProcessNepaliDateImport(invoice);

                            model.Invoices.Add(invoice);
                            rowIndex++;
                        }



                }
            }

            LoadDropdowns();

            // Pass errors to ViewBag so you can display in the View
            ViewBag.ImportErrors = importErrors;

            return View("CreateMultiple", model);
        }


        private int GetItemIdByName(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
                return 0;

            var normalizedInput = itemName.Trim().ToUpper();

            // Get all items as list and search in-memory:
            var items = _context.Items.AsEnumerable();

            var item = items.FirstOrDefault(i => i.ItemName != null && i.ItemName.Trim().ToUpper() == normalizedInput);

            return item?.ItemId ?? 0;
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
        private void ProcessNepaliDate(Invoice invoice)
        {
            if (string.IsNullOrWhiteSpace(invoice.NepaliMiti))
                return;

            var cleanedMiti = ConvertToEnglishDigits(invoice.NepaliMiti.Replace('-', '/'));
            var parts = cleanedMiti.Split('/');
            if (parts.Length != 3)
            {
                // Invalid format
                return;
            }

            int day, month, year;

            // Detect format: if first part has 4 digits, treat as yyyy/mm/dd
            if (parts[0].Length == 4)
            {
                // yyyy/mm/dd
                year = int.Parse(parts[0]);
                month = int.Parse(parts[1]);
                day = int.Parse(parts[2]);
            }
            else
            {
                // dd/mm/yyyy
                day = int.Parse(parts[0]);
                month = int.Parse(parts[1]);
                year = int.Parse(parts[2]);
            }

            var engDate = new NepaliDate(year, month, day).EnglishDate;
            invoice.NepaliMiti = cleanedMiti;
            invoice.Miti = engDate;
        }

        private void ProcessNepaliDateImport(Invoice invoice)
        {
            if (string.IsNullOrWhiteSpace(invoice.NepaliMiti))
                return;

            try
            {
                var cleanedMiti = ConvertToEnglishDigits(invoice.NepaliMiti.Replace('-', '/'));

                // Remove time part if present by splitting at space and taking the first segment
                var datePart = cleanedMiti.Split(' ')[0];

                var parts = datePart.Split('/');
                if (parts.Length != 3)
                {
                    // Invalid format
                    return;
                }

                int day, month, year;

                // Detect format: if first part has 4 digits, treat as yyyy/mm/dd
                if (parts[0].Length == 4)
                {
                    // yyyy/mm/dd
                    year = int.Parse(parts[0]);
                    month = int.Parse(parts[1]);
                    day = int.Parse(parts[2]);
                }
                else
                {
                    // dd/mm/yyyy
                    day = int.Parse(parts[0]);
                    month = int.Parse(parts[1]);
                    year = int.Parse(parts[2]);
                }

                var engDate = new NepaliDate(year, month, day).EnglishDate;

                // Assign the Gregorian equivalent to a DateTime property, e.g. Miti
                invoice.Miti = engDate;

                // Also update the cleaned NepaliMiti string if needed
                invoice.NepaliMiti = $"{year:D4}/{month:D2}/{day:D2}"; ;
            }
            catch (Exception ex)
            {
                // Log or handle parsing error
            }
        }


        private bool TryGetIntFromCell(IXLCell cell, out int result)
        {
            result = 0;
            if (cell.IsEmpty()) return false;

            if (cell.DataType == XLDataType.Number)
            {
                result = (int)cell.GetDouble();
                return true;
            }
            else
            {
                return int.TryParse(cell.GetString(), out result);
            }
        }
        private bool TryGetDecimalFromCell(IXLCell cell, out decimal result)
        {
            result = 0;
            if (cell.IsEmpty()) return false;

            if (cell.DataType == XLDataType.Number)
            {
                result = (decimal)cell.GetDouble();
                return true;
            }
            else
            {
                return decimal.TryParse(cell.GetString(), out result);
            }
        }


    }
}
