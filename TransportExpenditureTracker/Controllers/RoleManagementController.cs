using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Controllers;

[Authorize(Policy = "RequireSuperAdminRole")]
public class RoleManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("User Management", null));
        var users = await _userManager.Users.ToListAsync();
        var userRoles = new List<UserRolesViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userRoles.Add(new UserRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? "",
                Roles = roles.ToList()
            });
        }
        return View(userRoles);
    }

    public async Task<IActionResult> ManageRoles(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        var model = new List<ManageUserRolesViewModel>();
        foreach (var role in await _roleManager.Roles.ToListAsync())
        {
            model.Add(new ManageUserRolesViewModel
            {
                RoleName = role.Name ?? "",
                Selected = await _userManager.IsInRoleAsync(user, role.Name ?? "")
            });
        }
        ViewBag.UserEmail = user.Email;
        ViewBag.UserId = userId;
        return PartialView(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageRoles(List<ManageUserRolesViewModel> model, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Json(new { success = false, errors = new { UserId = new[] { "User not found." } } });
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        var selectedRoles = model.Where(r => r.Selected).Select(r => r.RoleName).ToList();
        if (selectedRoles.Count > 0)
        {
            await _userManager.AddToRolesAsync(user, selectedRoles);
        }
        return Json(new { success = true });
    }
}
