using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Principal;
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
            if (!ModelState.IsValid) return View(request);

            try
            {
                // 1. Llamamos a la API
                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", request);

                if (response.IsSuccessStatusCode)
                {
                    // 2. Primero leemos el resultado JSON
                    var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

                    if (result == null || !result.Exitoso)
                    {
                        ModelState.AddModelError("", result?.Mensaje ?? "Credenciales inválidas.");
                        return View(request);
                    }

                    // 3. Ahora que tenemos el 'result', creamos los Claims
                    var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, result.Usuario!.Id),
                new(ClaimTypes.Name, result.Usuario.NombreCompleto),
                new(ClaimTypes.Email, result.Usuario.Email),
                new("Token", result.Token!),
                new("RefreshToken", result.RefreshToken!)
            };

                    // Agregamos los roles si existen
                    if (result.Usuario.Roles != null)
                    {
                        foreach (var rol in result.Usuario.Roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, rol));
                        }
                    }

                    // 4. Creamos Identidad y Principal
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    // 5. Configuramos propiedades de autenticación
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = request.Recordarme,
                        ExpiresUtc = result.Expiracion
                    };

                    // 6. Iniciamos sesión en el sistema de Cookies de la Web
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                    // 7. Guardamos el Token en la sesión para futuras llamadas a la API
                    HttpContext.Session.SetString("JwtToken", result.Token!);

                    // 8. Cargamos permisos
                    await CargarPermisosEnSesion(result.Token!);

                    // 9. Redirección final
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Si la API responde con error, leemos el detalle
                    var errorDetail = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error en la API ({response.StatusCode}): {errorDetail}");
                    return View(request);
                }
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

        private async Task CargarPermisosEnSesion(string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("api/Permisos/mis-permisos");
                if (response.IsSuccessStatusCode)
                {
                    var permisosJson = await response.Content.ReadAsStringAsync();
                    HttpContext.Session.SetString("PermisosUsuario", permisosJson);
                }
                else
                {
                    // Si falla, guardar lista vacía para evitar errores
                    HttpContext.Session.SetString("PermisosUsuario", "[]");
                }
            }
            catch
            {
                HttpContext.Session.SetString("PermisosUsuario", "[]");
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult VerPermisos()
        {
            var permisosJson = HttpContext.Session.GetString("PermisosUsuario");
            var token = HttpContext.Session.GetString("JwtToken");

            return Content($"Token: {(string.IsNullOrEmpty(token) ? "NO HAY" : "SI HAY")}\n\nPermisos en sesión:\n{permisosJson ?? "NO HAY PERMISOS"}");
        }
    }
}