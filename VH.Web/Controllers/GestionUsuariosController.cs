using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;
using VH.Services.DTOs.Usuario;
using VH.Services.DTOs.Rol;

namespace VH.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Administrador")]
    public class GestionUsuariosController : Controller
    {
        private readonly HttpClient _httpClient;

        public GestionUsuariosController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
        }

        private void SetAuthHeader()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<IActionResult> Index(
            int pagina = 1, int tamano = ConsultaPaginada.TamanoPorOmision,
            string? buscar = null, bool? activo = null)
        {
            var consulta = new ConsultaPaginada { Pagina = pagina, Tamano = tamano, Buscar = buscar };
            ViewBag.FiltroActivo = activo;

            SetAuthHeader();
            try
            {
                var url = $"api/Usuarios/paginado?pagina={consulta.Pagina}&tamano={consulta.Tamano}";
                if (consulta.HayBusqueda) url += $"&buscar={Uri.EscapeDataString(consulta.TextoLimpio!)}";
                if (activo.HasValue) url += $"&activo={activo.Value.ToString().ToLowerInvariant()}";

                var response = await _httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                var pag = await response.Content.ReadFromJsonAsync<ResultadoPaginado<UsuarioResponseDto>>();
                return View(pag ?? ResultadoPaginado<UsuarioResponseDto>.Ninguno(consulta));
            }
            catch
            {
                ViewBag.Error = "Error al cargar usuarios";
                return View(ResultadoPaginado<UsuarioResponseDto>.Ninguno(consulta));
            }
        }

        public async Task<IActionResult> Create()
        {
            SetAuthHeader();
            await CargarRoles();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                await CargarRoles();
                return View(request);
            }

            SetAuthHeader();
            var response = await _httpClient.PostAsJsonAsync("api/Usuarios", request);

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Usuario creado exitosamente";
                return RedirectToAction(nameof(Index));
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", errorContent.Contains("mensaje") ?
                System.Text.Json.JsonDocument.Parse(errorContent).RootElement.GetProperty("mensaje").GetString() ?? "Error al crear usuario" :
                "Error al crear usuario");
            await CargarRoles();
            return View(request);
        }

        public async Task<IActionResult> Edit(string id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/Usuarios/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var usuario = await response.Content.ReadFromJsonAsync<UsuarioResponseDto>();
            await CargarRoles();

            var request = new UsuarioRequestDto(
                usuario!.Nombre,
                usuario.ApellidoPaterno,
                usuario.ApellidoMaterno,
                usuario.Email,
                usuario.UserName,
                null,
                usuario.Roles.ToList(),
                usuario.Activo
            );

            ViewBag.UsuarioId = id;
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UsuarioRequestDto request)
        {
            ModelState.Remove("Password");

            if (!ModelState.IsValid)
            {
                await CargarRoles();
                ViewBag.UsuarioId = id;
                return View(request);
            }

            SetAuthHeader();
            var response = await _httpClient.PutAsJsonAsync($"api/Usuarios/{id}", request);

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Usuario actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", errorContent.Contains("mensaje") ?
                System.Text.Json.JsonDocument.Parse(errorContent).RootElement.GetProperty("mensaje").GetString() ?? "Error al actualizar" :
                "Error al actualizar");
            await CargarRoles();
            ViewBag.UsuarioId = id;
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivo(string id)
        {
            SetAuthHeader();
            await _httpClient.PatchAsync($"api/Usuarios/{id}/toggle-activo", null);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            SetAuthHeader();
            await _httpClient.DeleteAsync($"api/Usuarios/{id}");
            TempData["Mensaje"] = "Usuario eliminado";
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarRoles()
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync("api/Roles");
            if (response.IsSuccessStatusCode)
            {
                var roles = await response.Content.ReadFromJsonAsync<IEnumerable<RolResponseDto>>();
                ViewBag.Roles = roles;
            }
            else
            {
                ViewBag.Roles = new List<RolResponseDto>();
            }
        }
    }
}