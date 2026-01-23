using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs.Rol;

namespace VH.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class GestionRolesController : Controller
    {
        private readonly HttpClient _httpClient;

        public GestionRolesController(IHttpClientFactory httpClientFactory)
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

        public async Task<IActionResult> Index()
        {
            SetAuthHeader();
            try
            {
                var response = await _httpClient.GetAsync("api/Roles");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                var roles = await response.Content.ReadFromJsonAsync<IEnumerable<RolResponseDto>>();
                return View(roles);
            }
            catch
            {
                ViewBag.Error = "Error al cargar roles";
                return View(new List<RolResponseDto>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RolRequestDto request)
        {
            if (!ModelState.IsValid) return View(request);

            SetAuthHeader();
            var response = await _httpClient.PostAsJsonAsync("api/Roles", request);

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Rol creado exitosamente";
                return RedirectToAction(nameof(Index));
            }

            var error = await response.Content.ReadFromJsonAsync<dynamic>();
            ModelState.AddModelError("", error?.mensaje?.ToString() ?? "Error al crear rol");
            return View(request);
        }

        public async Task<IActionResult> Edit(string id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/Roles/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var rol = await response.Content.ReadFromJsonAsync<RolResponseDto>();
            var request = new RolRequestDto(rol!.Name, rol.Descripcion, rol.Activo);
            ViewBag.RolId = id;
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, RolRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RolId = id;
                return View(request);
            }

            SetAuthHeader();
            var response = await _httpClient.PutAsJsonAsync($"api/Roles/{id}", request);

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Rol actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }

            var error = await response.Content.ReadFromJsonAsync<dynamic>();
            ModelState.AddModelError("", error?.mensaje?.ToString() ?? "Error al actualizar");
            ViewBag.RolId = id;
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            SetAuthHeader();
            await _httpClient.DeleteAsync($"api/Roles/{id}");
            TempData["Mensaje"] = "Rol eliminado";
            return RedirectToAction(nameof(Index));
        }
    }
}