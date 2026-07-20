using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using TransportExpenditureTracker.Converters;
using TransportExpenditureTracker.DataManagers.Interfaces;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;
using static TransportExpenditureTracker.Helper.ControllerHelpers;

namespace TransportExpenditureTracker.Controllers;

[Authorize(Roles = "Admin")]
public class ExpenseCategoriesController(IExpenseCategoryDataManager categoryDataManager, ExpenseCategoryConverter converter, IAuditService audit) : Controller
{
    private const string DeleteAction = "Delete";
    private static readonly string[] CategoryNameRequired = ["Category name is required."];
    private static readonly string[] CategoryNotFound = ["Category not found."];
    private static readonly string[] CategoryInUse = ["Cannot delete: category is referenced in existing expense records."];

    public async Task<IActionResult> Index()
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Categories", null));
        var categories = await categoryDataManager.GetAllAsync();
        var vms = categories.Select(converter.ToViewModel).ToList();
        var countMap = await categoryDataManager.GetHeaderCountsAsync();
        foreach (var vm in vms)
            vm.ExpenseCount = countMap.GetValueOrDefault(vm.CategoryId);
        return View(vms);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] ExpenseCategoryViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var category = new ExpenseCategory { CategoryName = vm.CategoryName };
            await categoryDataManager.AddAsync(category);
            return Json(new { success = true });
        }
        return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        var category = await categoryDataManager.GetByIdAsync(id);
        if (category == null) return NotFound();
        var vm = new ExpenseCategoryViewModel { CategoryId = category.CategoryId, CategoryName = category.CategoryName };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ExpenseCategoryViewModel vm)
    {
        if (id != vm.CategoryId) return NotFound();
        if (ModelState.IsValid)
        {
            var category = await categoryDataManager.GetByIdAsync(id);
            if (category == null) return NotFound();
            category.CategoryName = vm.CategoryName;
            await categoryDataManager.UpdateAsync(category);
            return RedirectToAction(nameof(Index));
        }
        return View(vm);
    }

    public async Task<IActionResult> Delete(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        var category = await categoryDataManager.GetByIdAsync(id);
        if (category == null) return NotFound();
        var vm = new ExpenseCategoryViewModel { CategoryId = category.CategoryId, CategoryName = category.CategoryName };
        ViewData["DeleteConfirm"] = $"Are you sure you want to delete category '{category.CategoryName}'?";
        return View(vm);
    }

    [HttpPost, ActionName(DeleteAction)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid request.";
            return RedirectToAction(nameof(Index));
        }
        var category = await categoryDataManager.GetByIdAsync(id);
        if (category != null)
        {
            var inUse = await categoryDataManager.IsReferencedAsync(id);
            if (inUse)
            {
                TempData["Error"] = $"Cannot delete '{category.CategoryName}': it is referenced in existing expense records.";
                return RedirectToAction(nameof(Index));
            }
            await categoryDataManager.DeleteAsync(id);
            await audit.LogAsync("ExpenseCategory", id.ToString(CultureInfo.InvariantCulture), DeleteAction, category.CategoryName, null, User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? "");
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetForEdit(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        var category = await categoryDataManager.GetByIdAsync(id);
        if (category == null) return NotFound();
        var vm = new ExpenseCategoryViewModel { CategoryId = category.CategoryId, CategoryName = category.CategoryName };
        return PartialView("_CategoryEditForm", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickUpdate([FromBody] QuickCategoryUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
        if (string.IsNullOrWhiteSpace(request.CategoryName))
            return Json(new { success = false, errors = new { categoryName = CategoryNameRequired } });

        var category = await categoryDataManager.GetByIdAsync(request.CategoryId);
        if (category == null)
            return Json(new { success = false, errors = new { general = CategoryNotFound } });

        category.CategoryName = request.CategoryName;
        await categoryDataManager.UpdateAsync(category);
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetDeleteInfo(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        var category = await categoryDataManager.GetByIdAsync(id);
        if (category == null) return NotFound();
        var vm = new ExpenseCategoryViewModel { CategoryId = category.CategoryId, CategoryName = category.CategoryName };
        return PartialView("_CategoryDeleteInfo", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickDelete([FromBody] QuickDeleteRequest request)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
        var category = await categoryDataManager.GetByIdAsync(request.Id);
        if (category != null)
        {
            var inUse = await categoryDataManager.IsReferencedAsync(request.Id);
            if (inUse)
                return Json(new { success = false, errors = new { general = CategoryInUse } });
            await categoryDataManager.DeleteAsync(request.Id);
            await audit.LogAsync("ExpenseCategory", request.Id.ToString(CultureInfo.InvariantCulture), DeleteAction, category.CategoryName, null, User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? "");
        }
        return Json(new { success = true });
    }
}
