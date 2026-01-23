using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VH.Services.DTOs.Auth;

namespace VH.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDto request, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(request);

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", request);
                var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

                if (result == null || !result.Exitoso)
                {
                    ModelState.AddModelError("", result?.Mensaje ?? "Error al iniciar sesión");
                    return View(request);
                }

                // Crear claims para la cookie
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, result.Usuario!.Id),
                    new(ClaimTypes.Name, result.Usuario.NombreCompleto),
                    new(ClaimTypes.Email, result.Usuario.Email),
                    new("Token", result.Token!),
                    new("RefreshToken", result.RefreshToken!)
                };

                foreach (var rol in result.Usuario.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, rol));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = request.Recordarme,
                    ExpiresUtc = result.Expiracion
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                // Guardar token en sesión
                HttpContext.Session.SetString("JwtToken", result.Token!);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error de conexión: " + ex.Message);
                return View(request);
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    await _httpClient.PostAsync("api/Auth/logout", null);
                }
            }
            catch { }

            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return RedirectToAction("Login");

            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"api/Usuarios/{userId}");
                if (!response.IsSuccessStatusCode)
                    return RedirectToAction("Index", "Home");

                var usuario = await response.Content.ReadFromJsonAsync<VH.Services.DTOs.Usuario.UsuarioResponseDto>();
                return View(usuario);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult CambiarPassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(CambioPasswordDto request)
        {
            if (!ModelState.IsValid)
                return View(request);

            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync("api/Auth/cambiar-password", request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Contraseña actualizada exitosamente";
                    return RedirectToAction("Perfil");
                }

                ModelState.AddModelError("", "No se pudo cambiar la contraseña. Verifique la contraseña actual.");
                return View(request);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
                return View(request);
            }
        }
    }
}