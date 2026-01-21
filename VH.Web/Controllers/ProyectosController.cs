using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;

namespace VH.Web.Controllers
{
    public class ProyectosController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProyectosController> _logger;

        public ProyectosController(IHttpClientFactory httpClientFactory, ILogger<ProyectosController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Llama al endpoint de la API
                var response = await _httpClient.GetAsync("api/proyectos");
                response.EnsureSuccessStatusCode(); // Lanza excepción si es 4xx o 5xx

                var proyectos = await response.Content.ReadFromJsonAsync<IEnumerable<ProyectoResponseDto>>();

                // Pasa la lista de DTOs a la vista
                return View(proyectos);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Error al conectar con la API.");
                // Manejo de error simple: muestra un mensaje de error
                ViewBag.ErrorMessage = $"Error al cargar proyectos: {e.Message}";
                return View(new List<ProyectoResponseDto>());
            }
        }

        public IActionResult Create()
        {
            // Devolvemos la vista asociada al formulario de creación.
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Create([Bind("Nombre,TipoObra,FechaInicio,PresupuestoTotal")] ProyectoRequestDto proyectoDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Serializa el DTO y lo envía a la API
                    var response = await _httpClient.PostAsJsonAsync("api/proyectos", proyectoDto);
                    response.EnsureSuccessStatusCode();

                    // Redirige al listado después de la creación exitosa
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Si la API falla, registramos el error y volvemos a mostrar el formulario
                    ModelState.AddModelError("", "Error al crear el proyecto: " + ex.Message);
                }
            }
            // Si el ModelState no es válido o hay un error, vuelve a la vista.
            return View(proyectoDto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"api/proyectos/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            // Obtener el DTO de respuesta
            var proyectoResponse = await response.Content.ReadFromJsonAsync<ProyectoResponseDto>();

            // Crear un DTO de Request a partir del DTO de Respuesta para llenar el formulario
            var proyectoRequest = new ProyectoRequestDto(
                proyectoResponse!.Nombre,
                proyectoResponse.TipoObra,
                proyectoResponse.FechaInicio,
                proyectoResponse.PresupuestoTotal
            );

            return View(proyectoRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Nombre,TipoObra,FechaInicio,PresupuestoTotal")] ProyectoRequestDto proyectoDto)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // La API requiere el DTO completo en el cuerpo de la petición PUT
                    var response = await _httpClient.PutAsJsonAsync($"api/proyectos/{id}", proyectoDto);
                    response.EnsureSuccessStatusCode();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el proyecto: " + ex.Message);
                }
            }
            return View(proyectoDto);
        }

        // GET: Proyectos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/proyectos/{id}");
                if (!response.IsSuccessStatusCode) return NotFound();
                var proyecto = await response.Content.ReadFromJsonAsync<ProyectoResponseDto>();
                return View(proyecto);
            }
            catch
            {
                return NotFound();
            }
        }
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.GetAsync($"api/proyectos/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            // Usamos el DTO de respuesta para mostrar la información en la vista de confirmación
            var proyecto = await response.Content.ReadFromJsonAsync<ProyectoResponseDto>();
            return View(proyecto);
        }

        // POST: Proyectos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/proyectos/{id}");

            if (!response.IsSuccessStatusCode)
            {
                // Si hay un error, redirigir a la vista de confirmación con un mensaje de error
                // (Necesitaríamos implementar la vista Delete.cshtml para manejar esto, 
                // por ahora, solo redirigimos a Index si falla o tiene éxito.)

                // Para fines de simplicidad, asumiremos que el error es manejado o la API devuelve éxito.
            }

            return RedirectToAction(nameof(Index));
        }

        //public IActionResult Details(int id)
        //{
        //    // Redirige al controlador de Partidas, pasando el ID del proyecto
        //    return RedirectToAction("Index", "Partidas", new { idProyecto = id });
        //}



    }
}