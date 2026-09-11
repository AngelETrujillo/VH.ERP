using Microsoft.AspNetCore.Authentication.Cookies;
using VH.Web.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Necesario para que TokenSesionHandler pueda leer la sesión de la petición actual.
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenSesionHandler>();

// CONFIGURACI�N DIN�MICA DE API
builder.Services.AddHttpClient("ApiERP", client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7088/";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddHttpMessageHandler<TokenSesionHandler>();

// Sesi�n - IMPORTANTE: Configurar correctamente
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".VHERP.Session";  // Agregar nombre expl�cito
    options.Cookie.SameSite = SameSiteMode.Lax;  // Agregar esto
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.Cookie.Name = ".VHERP.Auth";  // Agregar nombre expl�cito
    });

builder.Services.AddAuthorization();

// Cultura M�xico (moneda MXN)
var culturaMx = new System.Globalization.CultureInfo("es-MX");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culturaMx;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culturaMx;

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();  // Agregar para HTTPS
}

app.UseHttpsRedirection();  // Agregar esto
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();