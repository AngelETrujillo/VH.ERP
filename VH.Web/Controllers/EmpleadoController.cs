using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VH.Services.DTOs;
using VH.Web.Filters;

namespace VH.Web.Controllers
{
    [Authorize]
    [RequierePermiso("EMPLEADOS", "ver")]
    public class EmpleadoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EmpleadoController> _logger;

        public EmpleadoController(IHttpClientFactory httpClientFactory, ILogger<EmpleadoController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
            _logger = logger;
        }

        private void SetAuthHeader()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        private async Task<List<SelectListItem>> GetProyectosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/proyectos");
                if (response.IsSuccessStatusCode)
                {
                    var proyectos = await response.Content.ReadFromJsonAsync<IEnumerable<ProyectoResponseDto>>();
                    return proyectos?
                        .Select(p => new SelectListItem { Value = p.IdProyecto.ToString(), Text = p.Nombre })
                        .ToList() ?? new List<SelectListItem>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar proyectos para dropdown.");
            }
            return new List<SelectListItem>();
        }

        // GET: Empleado
        public async Task<IActionResult> Index()
        {
            SetAuthHeader();
            try
            {
                var response = await _httpClient.GetAsync("api/empleados");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                response.EnsureSuccessStatusCode();
                var empleados = await response.Content.ReadFromJsonAsync<IEnumerable<EmpleadoResponseDto>>();
                return View(empleados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar empleados");
                ViewBag.ErrorMessage = "Error al cargar los empleados";
                return View(new List<EmpleadoResponseDto>());
            }
        }

        // GET: Empleado/Details/5
        public async Task<IActionResult> Details(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/empleados/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var empleado = await response.Content.ReadFromJsonAsync<EmpleadoResponseDto>();
            return View(empleado);
        }

        // GET: Empleado/Create
        [RequierePermiso("EMPLEADOS", "crear")]
        public async Task<IActionResult> Create()
        {
            SetAuthHeader();
            ViewBag.Proyectos = await GetProyectosAsync();
            return View();
        }

        // POST: Empleado/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("EMPLEADOS", "crear")]
        public async Task<IActionResult> Create(EmpleadoRequestDto empleadoDto)
        {
            if (ModelState.IsValid)
            {
                SetAuthHeader();
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/empleados", empleadoDto);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Mensaje"] = "Empleado creado exitosamente";
                        return RedirectToAction(nameof(Index));
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", error);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear empleado");
                    ModelState.AddModelError("", "Error al crear el empleado");
                }
            }
            ViewBag.Proyectos = await GetProyectosAsync();
            return View(empleadoDto);
        }

        // GET: Empleado/Edit/5
        [RequierePermiso("EMPLEADOS", "editar")]
        public async Task<IActionResult> Edit(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/empleados/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var empleadoResponse = await response.Content.ReadFromJsonAsync<EmpleadoResponseDto>();
            var empleadoRequest = new EmpleadoRequestDto(
                empleadoResponse!.Nombre,
                empleadoResponse.ApellidoPaterno,
                empleadoResponse.ApellidoMaterno,
                empleadoResponse.NumeroNomina,
                empleadoResponse.Puesto,
                empleadoResponse.Activo,
                empleadoResponse.IdProyecto
            );

            ViewBag.Proyectos = await GetProyectosAsync();
            ViewBag.IdEmpleado = id;
            return View(empleadoRequest);
        }

        // POST: Empleado/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("EMPLEADOS", "editar")]
        public async Task<IActionResult> Edit(int id, EmpleadoRequestDto empleadoDto)
        {
            if (ModelState.IsValid)
            {
                SetAuthHeader();
                var response = await _httpClient.PutAsJsonAsync($"api/empleados/{id}", empleadoDto);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Empleado actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error al actualizar");
            }
            ViewBag.Proyectos = await GetProyectosAsync();
            ViewBag.IdEmpleado = id;
            return View(empleadoDto);
        }

        // GET: Empleado/Delete/5
        [RequierePermiso("EMPLEADOS", "eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/empleados/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var empleado = await response.Content.ReadFromJsonAsync<EmpleadoResponseDto>();
            return View(empleado);
        }

        // POST: Empleado/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequierePermiso("EMPLEADOS", "eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.DeleteAsync($"api/empleados/{id}");
            if (response.IsSuccessStatusCode)
                TempData["Mensaje"] = "Empleado eliminado";
            else
                TempData["Error"] = "No se pudo eliminar el empleado";

            return RedirectToAction(nameof(Index));
        }
    }
}