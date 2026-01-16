// VH.Web/Controllers/PartidasController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public async Task<IActionResult> Index(int idProyecto)
        {
            ViewBag.IdProyecto = idProyecto;

            // 1. Obtener los detalles del proyecto
            var proyectoResponse = await _httpClient.GetAsync($"api/proyectos/{idProyecto}");
            if (!proyectoResponse.IsSuccessStatusCode)
            {
                return NotFound($"Proyecto con ID {idProyecto} no encontrado.");
            }
            var proyecto = await proyectoResponse.Content.ReadFromJsonAsync<ProyectoResponseDto>();
            ViewBag.ProyectoNombre = proyecto?.Nombre;
            ViewBag.PresupuestoTotal = proyecto?.PresupuestoTotal;

            // 2. Obtener la lista de partidas del proyecto
            var response = await _httpClient.GetAsync($"api/proyectos/{idProyecto}/partidas");
            response.EnsureSuccessStatusCode();

            var partidas = await response.Content.ReadFromJsonAsync<IEnumerable<ConceptoPartidaResponseDto>>();

            return View(partidas);
        }

        // GET: Partidas/Create
        public async Task<IActionResult> Create(int idProyecto)
        {
            ViewBag.IdProyecto = idProyecto;
            await CargarUnidadesMedidaEnViewBag();
            return View();
        }

        // POST: Partidas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int idProyecto, ConceptoPartidaRequestDto partidaDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync($"api/proyectos/{idProyecto}/partidas", partidaDto);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index), new { idProyecto = idProyecto });
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error al crear la partida: {error}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear la partida: " + ex.Message);
                }
            }

            ViewBag.IdProyecto = idProyecto;
            await CargarUnidadesMedidaEnViewBag();
            return View(partidaDto);
        }

        // GET: Partidas/Edit/5
        public async Task<IActionResult> Edit(int? id, int idProyecto)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _httpClient.GetAsync($"api/proyectos/{idProyecto}/partidas/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var partida = await response.Content.ReadFromJsonAsync<ConceptoPartidaResponseDto>();
                if (partida == null)
                {
                    return NotFound();
                }

                // Convertir a RequestDto para el formulario
                var partidaRequest = new ConceptoPartidaRequestDto(
                    partida.Descripcion,
                    partida.IdUnidadMedida,
                    partida.CantidadEstimada
                );

                ViewBag.IdProyecto = idProyecto;
                ViewBag.IdPartida = id;
                await CargarUnidadesMedidaEnViewBag();
                return View(partidaRequest);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Partidas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int idProyecto, ConceptoPartidaRequestDto partidaDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PutAsJsonAsync($"api/proyectos/{idProyecto}/partidas/{id}", partidaDto);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index), new { idProyecto = idProyecto });
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error al actualizar: {error}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar la partida: " + ex.Message);
                }
            }

            ViewBag.IdProyecto = idProyecto;
            ViewBag.IdPartida = id;
            await CargarUnidadesMedidaEnViewBag();
            return View(partidaDto);
        }

        // GET: Partidas/Delete/5
        public async Task<IActionResult> Delete(int? id, int idProyecto)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _httpClient.GetAsync($"api/proyectos/{idProyecto}/partidas/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var partida = await response.Content.ReadFromJsonAsync<ConceptoPartidaResponseDto>();
                ViewBag.IdProyecto = idProyecto;
                return View(partida);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Partidas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int idProyecto)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/proyectos/{idProyecto}/partidas/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index), new { idProyecto = idProyecto });
                }
                TempData["ErrorMessage"] = "Error al eliminar la partida";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar la partida: " + ex.Message;
            }
            return RedirectToAction(nameof(Index), new { idProyecto = idProyecto });
        }

        // Método auxiliar para cargar unidades de medida
        private async Task CargarUnidadesMedidaEnViewBag()
        {
            try
            {
                var unidades = await _httpClient.GetFromJsonAsync<IEnumerable<UnidadMedidaResponseDto>>("api/unidadesmedida");
                ViewBag.UnidadesMedida = unidades?.Select(u => new SelectListItem
                {
                    Value = u.IdUnidadMedida.ToString(),
                    Text = $"{u.Abreviatura} - {u.Nombre}"
                }).ToList() ?? new List<SelectListItem>();
            }
            catch
            {
                ViewBag.UnidadesMedida = new List<SelectListItem>();
            }
        }
    }
}