using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SenacBuy.Web.Models;
using SenacBuy.Web.Services;

namespace SenacBuy.Web.Controllers;

/// <summary>
/// Controller responsável pelo fluxo de autenticação (Login, Logout e Registro).
/// Não acessa DbContext diretamente — toda comunicação ocorre via AuthApiService (HttpClient).
/// </summary>
public class AccountController : Controller
{
    private readonly AuthApiService _authService;

    public AccountController(AuthApiService authService)
        => _authService = authService;

    // ─────────────────────────────────────────────────────────────────────────
    // Login
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>GET /Account/Login — exibe o formulário de login</summary>
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        // Usuário já autenticado → redirecionar direto para Dashboard
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    /// <summary>POST /Account/Login — autentica via API e cria o cookie</summary>
    [HttpPost, ValidateAntiForgeryToken, AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var resultado = await _authService.LoginAsync(vm);

        if (resultado == null || !resultado.Sucesso)
        {
            ModelState.AddModelError("", resultado?.Mensagem ?? "Erro ao autenticar.");
            return View(vm);
        }

        // Montar Claims do usuário autenticado
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, resultado.Id.ToString()),
            new(ClaimTypes.Name,           resultado.Nome),
            new(ClaimTypes.Email,          resultado.Email),
            new("FotoPerfil",              resultado.FotoPerfil ?? string.Empty),
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });

        // Redirecionar para ReturnUrl ou Dashboard
        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);

        return RedirectToAction("Index", "Dashboard");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Logout
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>POST /Account/Logout — encerra a sessão e redireciona para Login</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Registro
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>GET /Account/Registrar — exibe o formulário de cadastro</summary>
    [AllowAnonymous]
    public IActionResult Registrar() => View(new RegistrarViewModel());

    /// <summary>POST /Account/Registrar — cria o usuário via API e redireciona para Login</summary>
    [HttpPost, ValidateAntiForgeryToken, AllowAnonymous]
    public async Task<IActionResult> Registrar(RegistrarViewModel vm, IFormFile? foto)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var (sucesso, mensagem) = await _authService.RegistrarAsync(vm, foto);

        if (!sucesso)
        {
            ModelState.AddModelError("", mensagem);
            return View(vm);
        }

        TempData["Sucesso"] = "Cadastro realizado! Faça login para continuar.";
        return RedirectToAction(nameof(Login));
    }
}
