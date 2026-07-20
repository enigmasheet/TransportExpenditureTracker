using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TransportExpenditureTracker.Helper;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    private const string CtlDashboard = "Dashboard";

    public IActionResult Index()
    {
        return RedirectToAction(nameof(Index), CtlDashboard);
    }

    public IActionResult Privacy()
    {
        this.SetBreadcrumbs(("Home", Url.Action(nameof(Index), CtlDashboard)), ("Privacy", null));
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        this.SetBreadcrumbs(("Home", Url.Action(nameof(Index), CtlDashboard)), ("Error", null));
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
