using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Dashboard");
    }

    public IActionResult Privacy()
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Privacy", null));
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        this.SetBreadcrumbs(("Home", Url.Action("Index", "Dashboard")), ("Error", null));
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
