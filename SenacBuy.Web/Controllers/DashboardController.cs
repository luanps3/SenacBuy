using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SenacBuy.Web.Models;
using SenacBuy.Web.Services;

namespace SenacBuy.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly DashboardApiService _dashboardService;

    public DashboardController(DashboardApiService dashboardService)
        => _dashboardService = dashboardService;

    public async Task<IActionResult> Index()
    {
        var vm = await _dashboardService.ObterAsync();
        return View(vm);
    }
}
