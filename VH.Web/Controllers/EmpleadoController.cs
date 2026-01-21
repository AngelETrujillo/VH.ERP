using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json; // Usamos esto para ReadFromJsonAsync y PostAsJsonAsync/PutAsJsonAsync
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using System.Text.Json;
using VH.Services.DTOs;

namespace VH.Web.Controllers
{
    public class EmpleadoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EmpleadoController> _logger;
        private const string ApiBaseUrl = "api/empleados";
        private const string ApiProyectosUrl = "api/proyectos";

        public EmpleadoController(IHttpClientFactory httpClientFactory, ILogger<EmpleadoController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
            _logger = logger;
        }

        // --- Método de Soporte para Obtener Proyectos (Para Dropdowns) ---
        private async Task<List<SelectListItem>> GetProyectosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(ApiProyectosUrl);
                response.EnsureSuccessStatusCode();
                var proyectos = await response.Content.ReadFromJsonAsync<IEnumerable<ProyectoResponseDto>>();
                return proyectos?
                    .Select(p => new SelectListItem { Value = p.IdProyecto.ToString(), Text = p.Nombre })
                    .ToList() ?? new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar proyectos para dropdown.");
                return new List<SelectListItem>();
            }
        }


        // 1. INDEX (Listar Empleados)
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync(ApiBaseUrl);
                response.EnsureSuccessStatusCode();

                var empleados = await response.Content.ReadFromJsonAsync<IEnumerable<EmpleadoResponseDto>>();
                return View(empleados);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Error al conectar con la API para listar empleados.");
                ViewBag.ErrorMessage = $"Error al cargar empleados: {e.Message}";
                return View(new List<EmpleadoResponseDto>());
            }
        }

        // 2. CREATE (GET: Muestra el formulario)
        public async Task<IActionResult> Create()
        {
            ViewBag.Proyectos = await GetProyectosAsync();
            return View();
        }

        // 2. CREATE (POST: Envía datos a la API)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,ApellidoPaterno,ApellidoMaterno,NumeroNomina,Puesto,Activo,IdProyecto")] EmpleadoRequestDto empleadoDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Usa PostAsJsonAsync que maneja la serialización
                    var response = await _httpClient.PostAsJsonAsync(ApiBaseUrl, empleadoDto);
                    response.EnsureSuccessStatusCode();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear el empleado: " + ex.Message);
                    _logger.LogError(ex, "Error POST al crear empleado.");
                }
            }

            // Si hay errores, recargar la lista de proyectos
            ViewBag.Proyectos = await GetProyectosAsync();
            return View(empleadoDto);
        }

        // DETAILS (Ver información del empleado)
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}/{id}");
                if (!response.IsSuccessStatusCode) return NotFound();
                var empleado = await response.Content.ReadFromJsonAsync<EmpleadoResponseDto>();
                return View(empleado);
            }
            catch
            {
                return NotFound();
            }
        }

        // 3. EDIT (GET: Carga datos para editar)
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"{ApiBaseUrl}/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            // El API nos devuelve un EmpleadoResponseDto (con objetos anidados)
            var empleadoResponse = await response.Content.ReadFromJsonAsync<EmpleadoResponseDto>();

            // Creamos manualmente el DTO de Request para llenar el formulario
            var empleadoRequest = new EmpleadoRequestDto(
                 empleadoResponse!.Nombre,
                 empleadoResponse.ApellidoPaterno,
                 empleadoResponse.ApellidoMaterno,
                 empleadoResponse.NumeroNomina,
                 empleadoResponse.Puesto,
                 empleadoResponse.Activo,
                 empleadoResponse.IdProyecto // FK del proyecto
            );

            ViewBag.Proyectos = await GetProyectosAsync();
            return View(empleadoRequest);
        }

        // 3. EDIT (POST: Envía datos actualizados a la API)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Nombre,ApellidoPaterno,ApellidoMaterno,NumeroNomina,Puesto,Activo,IdProyecto")] EmpleadoRequestDto empleadoDto)
        {
            if (id <= 0) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Usa PutAsJsonAsync
                    var response = await _httpClient.PutAsJsonAsync($"{ApiBaseUrl}/{id}", empleadoDto);
                    response.EnsureSuccessStatusCode();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el empleado: " + ex.Message);
                    _logger.LogError(ex, "Error PUT al actualizar empleado con ID: {Id}", id);
                }
            }

            ViewBag.Proyectos = await GetProyectosAsync();
            return View(empleadoDto);
        }

        // 4. DELETE (GET: Muestra confirmación)
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.GetAsync($"{ApiBaseUrl}/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            // Usamos el DTO de respuesta para mostrar la información completa (incluyendo nombre del proyecto)
            var empleado = await response.Content.ReadFromJsonAsync<EmpleadoResponseDto>();
            return View(empleado);
        }

        // 4. DELETE (POST: Confirma y elimina)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"{ApiBaseUrl}/{id}");

            if (!response.IsSuccessStatusCode)
            {
                // Si la API falla al eliminar, cargamos de nuevo los datos para mostrar el error en la vista
                var empleado = await _httpClient.GetFromJsonAsync<EmpleadoResponseDto>($"{ApiBaseUrl}/{id}");
                ModelState.AddModelError(string.Empty, $"Error al eliminar el empleado: {response.ReasonPhrase}");
                return View(empleado);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}