using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SenacBuy.Web.Models;

namespace SenacBuy.Web.Controllers;

/// <summary>
/// Controller público — acessível sem autenticação.
/// Usuários autenticados são redirecionados ao Dashboard.
/// </summary>
[AllowAnonymous]
public class HomeController : Controller
{
    /// <summary>Página inicial pública. Autenticados vão direto ao Dashboard.</summary>
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        return View();
    }

    /// <summary>Página Sobre pública.</summary>
    public IActionResult Sobre() => View();

    /// <summary>Página Contato pública.</summary>
    public IActionResult Contato() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
