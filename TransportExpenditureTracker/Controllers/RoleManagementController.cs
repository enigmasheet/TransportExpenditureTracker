using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.ViewModels;
namespace TransportExpenditureTracker.Controllers
{
    [Authorize(Policy = "RequireSuperAdminRole")]
    public class RoleManagementController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleManagementController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // List all users and their roles
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.AsNoTracking().ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var thisViewModel = new UserRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = new List<string>(await _userManager.GetRolesAsync(user))
                };
                userRolesViewModel.Add(thisViewModel);
            }

            return View(userRolesViewModel);
        }

        // GET: Manage roles for a specific user
        public async Task<IActionResult> Manage(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var allRoles = await _roleManager.Roles.AsNoTracking().ToListAsync();
            var model = new List<ManageUserRolesViewModel>();

            foreach (var role in allRoles)
            {
                var userRolesViewModel = new ManageUserRolesViewModel
                {
                    RoleName = role.Name,
                    Selected = await _userManager.IsInRoleAsync(user, role.Name)
                };
                model.Add(userRolesViewModel);
            }

            ViewBag.userId = userId;
            ViewBag.userEmail = user.Email;
            return View(model);
        }


        // POST: Update roles for a user
        [HttpPost]
        public async Task<IActionResult> Manage(List<ManageUserRolesViewModel> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }

            result = await _userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
    }

}
