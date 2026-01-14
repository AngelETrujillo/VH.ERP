// VH.Web/Controllers/PartidasController.cs
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;


namespace VH.Web.Controllers
{
    public class PartidasController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PartidasController> _logger;

        public PartidasController(IHttpClientFactory httpClientFactory, ILogger<PartidasController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
            _logger = logger;
        }

        // GET: /Proyectos/{idProyecto}/Partidas
        // Muestra la lista de partidas de un proyecto.
        public async Task<IActionResult> Index(int idProyecto)
        {
            ViewBag.IdProyecto = idProyecto;

            // 1. Obtener los detalles del proyecto (para el encabezado)
            var proyectoResponse = await _httpClient.GetAsync($"api/proyectos/{idProyecto}");
            if (!proyectoResponse.IsSuccessStatusCode)
            {
                return NotFound($"Proyecto con ID {idProyecto} no encontrado.");
            }
            var proyecto = await proyectoResponse.Content.ReadFromJsonAsync<ProyectoResponseDto>();
            ViewBag.ProyectoNombre = proyecto.Nombre;
            ViewBag.PresupuestoTotal = proyecto.PresupuestoTotal;

            // 2. Obtener la lista de partidas del proyecto
            var response = await _httpClient.GetAsync($"api/proyectos/{idProyecto}/partidas");
            response.EnsureSuccessStatusCode();

            var partidas = await response.Content.ReadFromJsonAsync<IEnumerable<ConceptoPartidaResponseDto>>();

            return View(partidas);
        }

        public IActionResult Create(int idProyecto)
        {
            // Almacenamos el ID del proyecto en ViewBag para que la vista POST pueda acceder a él.
            ViewBag.IdProyecto = idProyecto;
            // Creamos un DTO vacío para el formulario.
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int idProyecto, [Bind("Descripcion,UnidadMedida,CantidadEstimada")] ConceptoPartidaRequestDto partidaDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // La API consume la ruta anidada para crear la partida en el proyecto específico
                    var response = await _httpClient.PostAsJsonAsync($"api/proyectos/{idProyecto}/partidas", partidaDto);
                    response.EnsureSuccessStatusCode();

                    // Redirige al listado de partidas del proyecto padre
                    return RedirectToAction(nameof(Index), new { idProyecto = idProyecto });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear la partida: " + ex.Message);
                }
            }

            // Si falla, volvemos a mostrar la vista, asegurando que el ID del proyecto esté disponible.
            ViewBag.IdProyecto = idProyecto;
            return View(partidaDto);
        }

        public async Task<IActionResult> Edit(int? id, int idProyecto)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Obtener la partida específica desde la API (ruta anidada)
            var partida = await _httpClient.GetFromJsonAsync<ConceptoPartidaRequestDto>($"api/proyectos/{idProyecto}/partidas/{id}");

            if (partida == null)
            {
                return NotFound();
            }

            ViewBag.IdProyecto = idProyecto;
            return View(partida);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int idProyecto, [Bind("Descripcion,UnidadMedida,CantidadEstimada")] ConceptoPartidaRequestDto partidaDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Enviar la actualización a la API (ruta anidada)
                    var response = await _httpClient.PutAsJsonAsync($"api/proyectos/{idProyecto}/partidas/{id}", partidaDto);
                    response.EnsureSuccessStatusCode();

                    // Redirigir al listado de partidas del proyecto padre
                    return RedirectToAction(nameof(Index), new { idProyecto = idProyecto });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar la partida: " + ex.Message);
                }
            }

            // Si falla, volvemos a mostrar la vista.
            ViewBag.IdProyecto = idProyecto;
            return View(partidaDto);
        }

        public async Task<IActionResult> Delete(int? id, int idProyecto)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Obtener la partida para mostrar la confirmación
            var partida = await _httpClient.GetFromJsonAsync<ConceptoPartidaRequestDto>($"api/proyectos/{idProyecto}/partidas/{id}");

            if (partida == null)
            {
                return NotFound();
            }

            ViewBag.IdProyecto = idProyecto;
            // Creamos un DTO de respuesta para mostrar la información en la vista
            return View(new ConceptoPartidaResponseDto
            {
                IdPartida = id.Value, 
                Descripcion = partida.Descripcion,
                //UnidadMedida = partida.UnidadMedida,
                CantidadEstimada = partida.CantidadEstimada,
            });

        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int idProyecto)
        {
            try
            {
                // Enviar la solicitud de eliminación a la API (ruta anidada)
                var response = await _httpClient.DeleteAsync($"api/proyectos/{idProyecto}/partidas/{id}");
                response.EnsureSuccessStatusCode();

                // Redirigir al listado de partidas del proyecto padre
                return RedirectToAction(nameof(Index), new { idProyecto = idProyecto });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al eliminar la partida: " + ex.Message);
                return RedirectToAction(nameof(Index), new { idProyecto = idProyecto });
            }
        }
    }
}