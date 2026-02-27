using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VH.Web.Models;

namespace VH.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        // Log del token (tu código original)
        var token = HttpContext.Session.GetString("JwtToken");
        _logger.LogWarning("=== HOME: Token en sesión = {Token}",
            string.IsNullOrEmpty(token) ? "NULL/VACÍO" : "EXISTE (" + token.Length + " chars)");

        // Pasar URL de la API a la vista (nuevo)
        ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"]?.TrimEnd('/') ?? "https://localhost:7088";

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}