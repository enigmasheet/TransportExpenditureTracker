using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TransportExpenditureTracker.Converters;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;
using System.Globalization;
using static TransportExpenditureTracker.Helper.ControllerHelpers;

namespace TransportExpenditureTracker.Controllers;

[Authorize(Roles = "Admin")]
public class ExpenseCategoriesController(ApplicationDbContext ctx, ExpenseCategoryConverter converter, IAuditService audit) : Controller
{
    private const string DeleteAction = "Delete";
    private static readonly string[] CategoryNameRequired = ["Category name is required."];
    private static readonly string[] CategoryNotFound = ["Category not found."];
    private static readonly string[] CategoryInUse = ["Cannot delete: category is referenced in existing expense records."];

    public async Task<IActionResult> Index()
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Categories", null));
        var categories = await ctx.ExpenseCategories.OrderBy(c => c.CategoryName).ToListAsync();
        var vms = categories.Select(converter.ToViewModel).ToList();
        var counts = await ctx.ExpenseHeaders
            .GroupBy(h => h.CategoryId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToListAsync();
        var countMap = counts.ToDictionary(c => c.Id, c => c.Count);
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
            ctx.ExpenseCategories.Add(category);
            await ctx.SaveChangesAsync();
            return Json(new { success = true });
        }
        return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        var category = await ctx.ExpenseCategories.FindAsync(id);
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
            var category = await ctx.ExpenseCategories.FindAsync(id);
            if (category == null) return NotFound();
            category.CategoryName = vm.CategoryName;
            await ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(vm);
    }

    public async Task<IActionResult> Delete(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        var category = await ctx.ExpenseCategories.FindAsync(id);
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
        var category = await ctx.ExpenseCategories.FindAsync(id);
        if (category != null)
        {
            var inUse = await ctx.ExpenseHeaders.AnyAsync(h => h.CategoryId == id);
            if (inUse)
            {
                TempData["Error"] = $"Cannot delete '{category.CategoryName}': it is referenced in existing expense records.";
                return RedirectToAction(nameof(Index));
            }
            ctx.ExpenseCategories.Remove(category);
            await ctx.SaveChangesAsync();
            await audit.LogAsync("ExpenseCategory", id.ToString(CultureInfo.InvariantCulture), DeleteAction, category.CategoryName, null, User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? "");
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetForEdit(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        var category = await ctx.ExpenseCategories.FindAsync(id);
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

        var category = await ctx.ExpenseCategories.FindAsync(request.CategoryId);
        if (category == null)
            return Json(new { success = false, errors = new { general = CategoryNotFound } });

        category.CategoryName = request.CategoryName;
        await ctx.SaveChangesAsync();
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetDeleteInfo(int id)
    {
        if (!ModelState.IsValid) return NotFound();
        var category = await ctx.ExpenseCategories.FindAsync(id);
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
        var category = await ctx.ExpenseCategories.FindAsync(request.Id);
        if (category != null)
        {
            var inUse = await ctx.ExpenseHeaders.AnyAsync(h => h.CategoryId == request.Id);
            if (inUse)
                return Json(new { success = false, errors = new { general = CategoryInUse } });
            ctx.ExpenseCategories.Remove(category);
            await ctx.SaveChangesAsync();
            await audit.LogAsync("ExpenseCategory", request.Id.ToString(CultureInfo.InvariantCulture), DeleteAction, category.CategoryName, null, User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? "");
        }
        return Json(new { success = true });
    }

    public class QuickCategoryUpdateRequest
    {
        [System.Text.Json.Serialization.JsonRequired]
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}