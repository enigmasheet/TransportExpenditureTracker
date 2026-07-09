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
    private readonly IAuditService _auditService;
    private readonly ApplicationDbContext _ctx;
    private readonly ExpenseConverter _expenseConverter;

    public ExpensesController(
        IExpenseService expenseService,
        ICsvImportService csvImportService,
        IAuditService auditService,
        ApplicationDbContext ctx,
        ExpenseConverter expenseConverter)
    {
        _expenseService = expenseService;
        _csvImportService = csvImportService;
        _auditService = auditService;
        _ctx = ctx;
        _expenseConverter = expenseConverter;
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
        DropdownHelper.LoadFiscalYears(_ctx, ViewData);
        DropdownHelper.LoadNepaliMonths(ViewData);
        DropdownHelper.LoadSuppliers(_ctx, ViewData);
        DropdownHelper.LoadCategories(_ctx, ViewData);
        DropdownHelper.LoadPaymentMethods(ViewData);
        DropdownHelper.LoadItems(_ctx, ViewData);
        return View(new ExpenseEntryViewModel { Details = new List<ExpenseDetailViewModel> { new() } });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExpenseEntryViewModel vm)
    {
        if (ModelState.IsValid)
        {
            await _expenseService.AddAsync(vm, CurrentUserId);
            return RedirectToAction(nameof(Index));
        }
        DropdownHelper.LoadFiscalYears(_ctx, ViewData);
        DropdownHelper.LoadNepaliMonths(ViewData);
        DropdownHelper.LoadSuppliers(_ctx, ViewData);
        DropdownHelper.LoadCategories(_ctx, ViewData);
        DropdownHelper.LoadPaymentMethods(ViewData);
        DropdownHelper.LoadItems(_ctx, ViewData);
        return View(vm);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entryVm = await _expenseService.GetByIdAsync(id);
        if (entryVm == null) return NotFound();
        DropdownHelper.LoadFiscalYears(_ctx, ViewData);
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
            await _expenseService.UpdateAsync(vm, CurrentUserId);
            return RedirectToAction(nameof(Index));
        }
        DropdownHelper.LoadFiscalYears(_ctx, ViewData);
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
        return View("ImportPreview", preview);
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
        var autoCreate = TempData["AutoCreate"] is bool b && b;
        var result = await _csvImportService.ImportAsync(preview, CurrentUserId, autoCreate);
        TempData["ImportResult"] = $"{result.Inserted} records imported, {result.Skipped} skipped, {result.Errors} errors.";
        return RedirectToAction(nameof(Index));
    }
}
