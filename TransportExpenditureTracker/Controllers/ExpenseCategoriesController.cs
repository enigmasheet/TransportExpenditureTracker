using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Controllers;

[Authorize(Roles = "Admin")]
public class ExpenseCategoriesController : Controller
{
    private readonly ApplicationDbContext _ctx;

    public ExpenseCategoriesController(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _ctx.ExpenseCategories.OrderBy(c => c.CategoryName).ToListAsync();
        return View(categories);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExpenseCategoryViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var category = new ExpenseCategory { CategoryName = vm.CategoryName };
            _ctx.ExpenseCategories.Add(category);
            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(vm);
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
}
