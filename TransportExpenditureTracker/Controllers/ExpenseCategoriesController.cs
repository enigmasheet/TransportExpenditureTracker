using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Converters;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Controllers;

[Authorize(Roles = "Admin")]
public class ExpenseCategoriesController : Controller
{
    private readonly ApplicationDbContext _ctx;
    private readonly ExpenseCategoryConverter _converter;

    public ExpenseCategoriesController(ApplicationDbContext ctx, ExpenseCategoryConverter converter)
    {
        _ctx = ctx;
        _converter = converter;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _ctx.ExpenseCategories.OrderBy(c => c.CategoryName).ToListAsync();
        var vms = categories.Select(_converter.ToViewModel).ToList();
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
            _ctx.ExpenseCategories.Add(category);
            await _ctx.SaveChangesAsync();
            return Json(new { success = true });
        }
        return Json(new { success = false, errors = GetModelStateErrors(ModelState) });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _ctx.ExpenseCategories.FindAsync(id);
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
            var category = await _ctx.ExpenseCategories.FindAsync(id);
            if (category == null) return NotFound();
            category.CategoryName = vm.CategoryName;
            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(vm);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var category = await _ctx.ExpenseCategories.FindAsync(id);
        if (category == null) return NotFound();
        var vm = new ExpenseCategoryViewModel { CategoryId = category.CategoryId, CategoryName = category.CategoryName };
        ViewData["DeleteConfirm"] = $"Are you sure you want to delete category '{category.CategoryName}'?";
        return View(vm);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _ctx.ExpenseCategories.FindAsync(id);
        if (category != null)
        {
            _ctx.ExpenseCategories.Remove(category);
            await _ctx.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetForEdit(int id)
    {
        var category = await _ctx.ExpenseCategories.FindAsync(id);
        if (category == null) return NotFound();
        var vm = new ExpenseCategoryViewModel { CategoryId = category.CategoryId, CategoryName = category.CategoryName };
        return PartialView("_CategoryEditForm", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickUpdate([FromBody] QuickCategoryUpdateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CategoryName))
            return Json(new { success = false, errors = new { categoryName = new[] { "Category name is required." } } });

        var category = await _ctx.ExpenseCategories.FindAsync(request.CategoryId);
        if (category == null)
            return Json(new { success = false, errors = new { general = new[] { "Category not found." } } });

        category.CategoryName = request.CategoryName;
        await _ctx.SaveChangesAsync();
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetDeleteInfo(int id)
    {
        var category = await _ctx.ExpenseCategories.FindAsync(id);
        if (category == null) return NotFound();
        var vm = new ExpenseCategoryViewModel { CategoryId = category.CategoryId, CategoryName = category.CategoryName };
        return PartialView("_CategoryDeleteInfo", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickDelete([FromBody] QuickDeleteRequest request)
    {
        var category = await _ctx.ExpenseCategories.FindAsync(request.Id);
        if (category != null)
        {
            _ctx.ExpenseCategories.Remove(category);
            await _ctx.SaveChangesAsync();
        }
        return Json(new { success = true });
    }

    private static Dictionary<string, string[]> GetModelStateErrors(ModelStateDictionary modelState)
    {
        return modelState
            .Where(kv => kv.Value != null && kv.Value.Errors.Count > 0)
            .ToDictionary(
                kv => char.ToLowerInvariant(kv.Key[0]) + kv.Key.Substring(1),
                kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );
    }

    public class QuickCategoryUpdateRequest
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    public class QuickDeleteRequest
    {
        public int Id { get; set; }
    }
}