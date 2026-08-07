using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;
using TransportExpenditureTracker.Converters;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Controllers;

[Authorize]
public partial class ExpensesController(IExpenseService expenseService, ICsvImportService csvImportService, IExpenseDataManager expenseDataManager, ExpenseConverter expenseConverter, ILogger<ExpensesController> logger) : Controller
{
    private const string CtlDashboard = "Dashboard";
    private const string ExpensesLabel = "Expenses";
    private const string HomeLabel = "Home";

    private string CurrentUserId => HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

    public async Task<IActionResult> Index()
    {
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ExpensesLabel, null));
        var expenses = await expenseService.GetAllAsync();
        return View(expenses);
    }

    public async Task<IActionResult> Details(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ExpensesLabel, Url.Action(nameof(Index))), ("Details", null));
        var entryVm = await expenseService.GetByIdAsync(id);
        if (entryVm == null) return NotFound();
        return View(entryVm);
    }

    public async Task<IActionResult> Create()
    {
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ExpensesLabel, Url.Action(nameof(Index))), ("New Batch Entry", null));
        var suppliers = await expenseDataManager.GetSuppliersAsync();
        DropdownHelper.LoadSuppliers(suppliers, ViewData);
        var categories = await expenseDataManager.GetCategoriesAsync();
        DropdownHelper.LoadCategories(categories, ViewData);
        DropdownHelper.LoadPaymentMethods(ViewData);
        var items = await expenseDataManager.GetItemsAsync();
        DropdownHelper.LoadItems(items, ViewData);

        var vm = new ExpenseBatchViewModel
        {
            Rows = [.. Enumerable.Range(1, 5).Select(i => new ExpenseBatchRowViewModel { RowIndex = i })]
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExpenseBatchViewModel vm)
    {
        vm.Rows = [.. vm.Rows.Where(r => !string.IsNullOrWhiteSpace(r.InvoiceNo) || r.Quantity > 0 || r.Rate > 0)];

        var rowKeys = ModelState.Keys.Where(k => k.StartsWith("Rows[", StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var key in rowKeys) ModelState.Remove(key);
        TryValidateModel(vm);

        if (ModelState.IsValid)
        {
            var result = await expenseService.BatchCreateAsync(vm, CurrentUserId);
            Logs.BatchCreateCompleted(logger, CurrentUserId, result.Inserted, result.Skipped, result.Errors);
            TempData["Success"] = $"{result.Inserted} expenses created, {result.Skipped} skipped, {result.Errors} errors.";
            if (result.ErrorMessages.Count > 0)
                TempData["Error"] = string.Join("; ", result.ErrorMessages);
            if (result.SkippedReasons.Count > 0)
                TempData["Warning"] = string.Join("; ", result.SkippedReasons);
            return RedirectToAction(nameof(Index));
        }

        Logs.BatchCreateValidationFailed(logger, CurrentUserId, string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

        var suppliers = await expenseDataManager.GetSuppliersAsync();
        DropdownHelper.LoadSuppliers(suppliers, ViewData);
        var items = await expenseDataManager.GetItemsAsync();
        DropdownHelper.LoadItems(items, ViewData);
        return View(vm);
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ExpensesLabel, Url.Action(nameof(Index))), ("Edit", null));
        var entryVm = await expenseService.GetByIdAsync(id);
        if (entryVm == null) return NotFound();
        var fiscalYears = await expenseDataManager.GetFiscalYearsAsync();
        DropdownHelper.LoadFiscalYears(fiscalYears, ViewData, entryVm.FiscalYearId);
        DropdownHelper.LoadNepaliMonths(ViewData);
        var suppliers = await expenseDataManager.GetSuppliersAsync();
        DropdownHelper.LoadSuppliers(suppliers, ViewData, entryVm.SupplierId);
        var categories = await expenseDataManager.GetCategoriesAsync();
        DropdownHelper.LoadCategories(categories, ViewData, entryVm.CategoryId);
        DropdownHelper.LoadPaymentMethods(ViewData);
        var items = await expenseDataManager.GetItemsAsync();
        DropdownHelper.LoadItems(items, ViewData);
        return View(entryVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ExpenseEntryViewModel vm)
    {
        if (id != vm.ExpenseId) return NotFound();
        if (ModelState.IsValid)
        {
            if (await expenseService.IsDuplicateInvoiceAsync(vm.InvoiceNo, vm.SupplierId, vm.FiscalYearId, vm.ExpenseId))
            {
                ModelState.AddModelError(nameof(vm.InvoiceNo), "An expense with this invoice number already exists for the selected supplier in this fiscal year.");
            }
            else
            {
                if (!await expenseService.UpdateAsync(vm, CurrentUserId))
                {
                    TempData["Error"] = "Expense not found or you do not have permission to edit it.";
                    return RedirectToAction(nameof(Index));
                }
                TempData["Success"] = "Expense updated successfully.";
                return RedirectToAction(nameof(Index));
            }
        }
        var fiscalYears = await expenseDataManager.GetFiscalYearsAsync();
        DropdownHelper.LoadFiscalYears(fiscalYears, ViewData, vm.FiscalYearId);
        DropdownHelper.LoadNepaliMonths(ViewData);
        var suppliers = await expenseDataManager.GetSuppliersAsync();
        DropdownHelper.LoadSuppliers(suppliers, ViewData, vm.SupplierId);
        var categories = await expenseDataManager.GetCategoriesAsync();
        DropdownHelper.LoadCategories(categories, ViewData, vm.CategoryId);
        DropdownHelper.LoadPaymentMethods(ViewData);
        var items = await expenseDataManager.GetItemsAsync();
        DropdownHelper.LoadItems(items, ViewData);
        return View(vm);
    }

    public async Task<IActionResult> Delete(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ExpensesLabel, Url.Action(nameof(Index))), ("Delete", null));
        var entryVm = await expenseService.GetByIdAsync(id);
        if (entryVm == null) return NotFound();
        return View(entryVm);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid request.";
            return RedirectToAction(nameof(Index));
        }
        if (!await expenseService.DeleteAsync(id, CurrentUserId))
        {
            TempData["Error"] = "Expense not found or you do not have permission to delete it.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Search(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            var all = await expenseService.GetAllAsync();
            return PartialView("_SearchResults", all);
        }
        var results = await expenseDataManager.SearchAsync(term);
        var vms = results.Select(e => expenseConverter.ToHeaderViewModel(e)).ToList();
        return PartialView("_SearchResults", vms);
    }

    public IActionResult Import()
    {
        this.SetBreadcrumbs((HomeLabel, Url.Action(nameof(Index), CtlDashboard)), (ExpensesLabel, Url.Action(nameof(Index))), ("Import", null));
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile file, bool autoCreate)
    {
        if (!ModelState.IsValid)
            return View();
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "Please select a file to import.");
            return View();
        }
        var preview = await csvImportService.PreviewAsync(file);
        TempData["PreviewData"] = JsonSerializer.Serialize(preview);
        TempData["AutoCreate"] = autoCreate;
        TempData["HasWarnings"] = preview.Rows.Any(w => w.IsVatMismatch || w.IsTotalMismatch);
        return View("Import", preview);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportConfirm()
    {
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Import));
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
        var result = await csvImportService.ImportAsync(preview, CurrentUserId, autoCreate);
        TempData["ImportResult"] = $"{result.Inserted} records imported, {result.Skipped} skipped, {result.Errors} errors.";
        if (result.WarningCount > 0)
        {
            TempData["ImportWarnings"] = string.Join("; ", result.Warnings);
        }
        return RedirectToAction(nameof(Index));
    }

    private static partial class Logs
    {
        [LoggerMessage(LogLevel.Information, Message = "Batch create by {User}: {Inserted} inserted, {Skipped} skipped, {Errors} errors")]
        public static partial void BatchCreateCompleted(ILogger logger, string user, int inserted, int skipped, int errors);

        [LoggerMessage(LogLevel.Warning, Message = "Batch create validation failed for {User}: {Errors}")]
        public static partial void BatchCreateValidationFailed(ILogger logger, string user, string errors);
    }
}
