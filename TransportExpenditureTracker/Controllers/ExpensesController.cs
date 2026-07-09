using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TransportExpenditureTracker.Converters;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Controllers;

[Authorize]
public class ExpensesController : Controller
{
    private readonly IExpenseService _expenseService;
    private readonly ICsvImportService _csvImportService;
    private readonly ApplicationDbContext _ctx;
    private readonly ExpenseConverter _expenseConverter;
    private readonly ILogger<ExpensesController> _logger;

    public ExpensesController(
        IExpenseService expenseService,
        ICsvImportService csvImportService,
        ApplicationDbContext ctx,
        ExpenseConverter expenseConverter,
        ILogger<ExpensesController> logger)
    {
        _expenseService = expenseService;
        _csvImportService = csvImportService;
        _ctx = ctx;
        _expenseConverter = expenseConverter;
        _logger = logger;
    }

    private string CurrentUserId => HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

    public async Task<IActionResult> Index()
    {
        var expenses = await _expenseService.GetAllAsync();
        return View(expenses);
    }

    public async Task<IActionResult> Details(int id)
    {
        var entryVm = await _expenseService.GetByIdAsync(id);
        if (entryVm == null) return NotFound();
        return View(entryVm);
    }

    public IActionResult Create()
    {
        DropdownHelper.LoadSuppliers(_ctx, ViewData);
        DropdownHelper.LoadCategories(_ctx, ViewData);
        DropdownHelper.LoadPaymentMethods(ViewData);
        DropdownHelper.LoadItems(_ctx, ViewData);

        var vm = new ExpenseBatchViewModel
        {
            Rows = Enumerable.Range(1, 5).Select(i => new ExpenseBatchRowViewModel { RowIndex = i }).ToList()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExpenseBatchViewModel vm)
    {
        vm.Rows = vm.Rows.Where(r => !string.IsNullOrWhiteSpace(r.InvoiceNo) || r.Quantity > 0 || r.Rate > 0).ToList();

        // Clear ModelState errors for removed rows and re-validate
        var rowKeys = ModelState.Keys.Where(k => k.StartsWith("Rows[")).ToList();
        foreach (var key in rowKeys) ModelState.Remove(key);
        TryValidateModel(vm);

        if (ModelState.IsValid)
        {
            var result = await _expenseService.BatchCreateAsync(vm, CurrentUserId);
            _logger.LogInformation("Batch create by {User}: {Inserted} inserted, {Skipped} skipped, {Errors} errors",
                CurrentUserId, result.Inserted, result.Skipped, result.Errors);
            TempData["Success"] = $"{result.Inserted} expenses created, {result.Skipped} skipped, {result.Errors} errors.";
            if (result.ErrorMessages.Count > 0)
                TempData["Error"] = string.Join("; ", result.ErrorMessages);
            if (result.SkippedReasons.Count > 0)
                TempData["Warning"] = string.Join("; ", result.SkippedReasons);
            return RedirectToAction(nameof(Index));
        }

        _logger.LogWarning("Batch create validation failed for {User}: {Errors}",
            CurrentUserId, string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

        DropdownHelper.LoadSuppliers(_ctx, ViewData);
        DropdownHelper.LoadItems(_ctx, ViewData);
        return View(vm);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entryVm = await _expenseService.GetByIdAsync(id);
        if (entryVm == null) return NotFound();
        DropdownHelper.LoadFiscalYears(_ctx, ViewData, entryVm.FiscalYearId);
        DropdownHelper.LoadNepaliMonths(ViewData);
        DropdownHelper.LoadSuppliers(_ctx, ViewData, entryVm.SupplierId);
        DropdownHelper.LoadCategories(_ctx, ViewData, entryVm.CategoryId);
        DropdownHelper.LoadPaymentMethods(ViewData);
        DropdownHelper.LoadItems(_ctx, ViewData);
        return View(entryVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ExpenseEntryViewModel vm)
    {
        if (id != vm.ExpenseId) return NotFound();
        if (ModelState.IsValid)
        {
            if (await _expenseService.IsDuplicateInvoiceAsync(vm.InvoiceNo, vm.SupplierId, vm.FiscalYearId, vm.ExpenseId))
            {
                ModelState.AddModelError(nameof(vm.InvoiceNo), "An expense with this invoice number already exists for the selected supplier in this fiscal year.");
            }
            else
            {
                await _expenseService.UpdateAsync(vm, CurrentUserId);
                TempData["Success"] = "Expense updated successfully.";
                return RedirectToAction(nameof(Index));
            }
        }
        DropdownHelper.LoadFiscalYears(_ctx, ViewData, vm.FiscalYearId);
        DropdownHelper.LoadNepaliMonths(ViewData);
        DropdownHelper.LoadSuppliers(_ctx, ViewData, vm.SupplierId);
        DropdownHelper.LoadCategories(_ctx, ViewData, vm.CategoryId);
        DropdownHelper.LoadPaymentMethods(ViewData);
        DropdownHelper.LoadItems(_ctx, ViewData);
        return View(vm);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var entryVm = await _expenseService.GetByIdAsync(id);
        if (entryVm == null) return NotFound();
        return View(entryVm);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _expenseService.DeleteAsync(id, CurrentUserId);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Search(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            var all = await _expenseService.GetAllAsync();
            return PartialView("_SearchResults", all);
        }
        var results = await _ctx.ExpenseHeaders
            .Include(e => e.Supplier)
            .Include(e => e.Category)
            .Where(e => e.InvoiceNo.Contains(term) || e.Supplier.SupplierName.Contains(term) || (e.Remarks != null && e.Remarks.Contains(term)))
            .ToListAsync();
        var vms = results.Select(e => _expenseConverter.ToHeaderViewModel(e)).ToList();
        return PartialView("_SearchResults", vms);
    }

    public IActionResult Import()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile file, bool autoCreate)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "Please select a file to import.");
            return View();
        }
        var preview = await _csvImportService.PreviewAsync(file);
        TempData["PreviewData"] = JsonSerializer.Serialize(preview);
        TempData["AutoCreate"] = autoCreate;
        TempData["HasWarnings"] = preview.Rows.Any(w => w.IsVatMismatch || w.IsTotalMismatch);
        return View("Import", preview);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportConfirm()
    {
        var json = TempData["PreviewData"] as string;
        if (string.IsNullOrEmpty(json))
        {
            return RedirectToAction(nameof(Import));
        }
        var preview = JsonSerializer.Deserialize<CsvPreviewViewModel>(json);
        if (preview == null)
        {
            return RedirectToAction(nameof(Import));
        }

        var validCount = preview.Rows.Count(r => !r.IsDuplicate && !r.IsCrossRowDuplicate && string.IsNullOrEmpty(r.ValidationError));
        if (validCount == 0)
        {
            TempData["Error"] = "No valid rows to import. All rows have errors or are duplicates.";
            return RedirectToAction(nameof(Index));
        }

        var autoCreate = TempData["AutoCreate"] is bool b && b;
        var result = await _csvImportService.ImportAsync(preview, CurrentUserId, autoCreate);
        TempData["ImportResult"] = $"{result.Inserted} records imported, {result.Skipped} skipped, {result.Errors} errors.";
        if (result.WarningCount > 0)
        {
            TempData["ImportWarnings"] = string.Join("; ", result.Warnings);
        }
        return RedirectToAction(nameof(Index));
    }
}