using Microsoft.AspNetCore.Authentication.Cookies;
using SenacBuy.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Registrar MVC
builder.Services.AddControllersWithViews();

// ── Cookie Authentication ─────────────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath        = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan   = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly  = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Name      = "SenacBuy.Auth";
    });

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// HttpClient configurado para consumir a API SenacBuy
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000";
builder.Services.AddHttpClient("SenacBuyAPI", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Registrar Services da camada Web
builder.Services.AddScoped<AuthApiService>();
builder.Services.AddScoped<ClienteApiService>();
builder.Services.AddScoped<ProdutoApiService>();
builder.Services.AddScoped<PedidoApiService>();
builder.Services.AddScoped<UsuarioApiService>();
builder.Services.AddScoped<CategoriaApiService>();
builder.Services.AddScoped<DashboardApiService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();   // ← deve vir ANTES de UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
