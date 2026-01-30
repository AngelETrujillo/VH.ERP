using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VH.Services.DTOs;
using VH.Web.Filters;

namespace VH.Web.Controllers
{
    [Authorize]
    [RequierePermiso("PROYECTOS", "ver")]
    public class PartidasController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PartidasController> _logger;

        public PartidasController(IHttpClientFactory httpClientFactory, ILogger<PartidasController> logger)
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

        // GET: /Partidas?idProyecto=1
        public async Task<IActionResult> Index(int idProyecto)
        {
            SetAuthHeader();
            try
            {
                var proyectoResponse = await _httpClient.GetAsync($"api/proyectos/{idProyecto}");
                if (proyectoResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                if (!proyectoResponse.IsSuccessStatusCode)
                    return NotFound($"Proyecto con ID {idProyecto} no encontrado.");

                var proyecto = await proyectoResponse.Content.ReadFromJsonAsync<ProyectoResponseDto>();
                ViewBag.IdProyecto = idProyecto;
                ViewBag.ProyectoNombre = proyecto?.Nombre;
                ViewBag.PresupuestoTotal = proyecto?.PresupuestoTotal;

                var response = await _httpClient.GetAsync($"api/proyectos/{idProyecto}/partidas");
                response.EnsureSuccessStatusCode();
                var partidas = await response.Content.ReadFromJsonAsync<IEnumerable<ConceptoPartidaResponseDto>>();

                return View(partidas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar partidas");
                ViewBag.ErrorMessage = "Error al cargar las partidas";
                return View(new List<ConceptoPartidaResponseDto>());
            }
        }

        // GET: Partidas/Create
        [RequierePermiso("PROYECTOS", "crear")]
        public async Task<IActionResult> Create(int idProyecto)
        {
            SetAuthHeader();
            ViewBag.IdProyecto = idProyecto;
            await CargarUnidadesMedidaEnViewBag();
            return View();
        }

        // POST: Partidas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("PROYECTOS", "crear")]
        public async Task<IActionResult> Create(int idProyecto, ConceptoPartidaRequestDto partidaDto)
        {
            if (ModelState.IsValid)
            {
                SetAuthHeader();
                try
                {
                    var response = await _httpClient.PostAsJsonAsync($"api/proyectos/{idProyecto}/partidas", partidaDto);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Mensaje"] = "Partida creada exitosamente";
                        return RedirectToAction(nameof(Index), new { idProyecto });
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", error);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear partida");
                    ModelState.AddModelError("", "Error al crear la partida");
                }
            }

            ViewBag.IdProyecto = idProyecto;
            await CargarUnidadesMedidaEnViewBag();
            return View(partidaDto);
        }

        // GET: Partidas/Edit/5
        [RequierePermiso("PROYECTOS", "editar")]
        public async Task<IActionResult> Edit(int id, int idProyecto)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/proyectos/{idProyecto}/partidas/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var partida = await response.Content.ReadFromJsonAsync<ConceptoPartidaResponseDto>();
            var partidaRequest = new ConceptoPartidaRequestDto(
                partida!.Descripcion,
                partida.IdUnidadMedida,
                partida.CantidadEstimada
            );

            ViewBag.IdProyecto = idProyecto;
            ViewBag.IdPartida = id;
            await CargarUnidadesMedidaEnViewBag();
            return View(partidaRequest);
        }

        // POST: Partidas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("PROYECTOS", "editar")]
        public async Task<IActionResult> Edit(int id, int idProyecto, ConceptoPartidaRequestDto partidaDto)
        {
            if (ModelState.IsValid)
            {
                SetAuthHeader();
                var response = await _httpClient.PutAsJsonAsync($"api/proyectos/{idProyecto}/partidas/{id}", partidaDto);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Partida actualizada exitosamente";
                    return RedirectToAction(nameof(Index), new { idProyecto });
                }
                ModelState.AddModelError("", "Error al actualizar");
            }

            ViewBag.IdProyecto = idProyecto;
            ViewBag.IdPartida = id;
            await CargarUnidadesMedidaEnViewBag();
            return View(partidaDto);
        }

        // GET: Partidas/Delete/5
        [RequierePermiso("PROYECTOS", "eliminar")]
        public async Task<IActionResult> Delete(int id, int idProyecto)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/proyectos/{idProyecto}/partidas/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var partida = await response.Content.ReadFromJsonAsync<ConceptoPartidaResponseDto>();
            ViewBag.IdProyecto = idProyecto;
            return View(partida);
        }

        // POST: Partidas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequierePermiso("PROYECTOS", "eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id, int idProyecto)
        {
            SetAuthHeader();
            var response = await _httpClient.DeleteAsync($"api/proyectos/{idProyecto}/partidas/{id}");
            if (response.IsSuccessStatusCode)
                TempData["Mensaje"] = "Partida eliminada";
            else
                TempData["Error"] = "No se pudo eliminar la partida";

            return RedirectToAction(nameof(Index), new { idProyecto });
        }

        private async Task CargarUnidadesMedidaEnViewBag()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/unidadesmedida");
                if (response.IsSuccessStatusCode)
                {
                    var unidades = await response.Content.ReadFromJsonAsync<IEnumerable<UnidadMedidaResponseDto>>();
                    ViewBag.UnidadesMedida = unidades?.Select(u => new SelectListItem
                    {
                        Value = u.IdUnidadMedida.ToString(),
                        Text = $"{u.Abreviatura} - {u.Nombre}"
                    }).ToList() ?? new List<SelectListItem>();
                }
                else
                {
                    ViewBag.UnidadesMedida = new List<SelectListItem>();
                }
            }
            catch
            {
                ViewBag.UnidadesMedida = new List<SelectListItem>();
            }
        }
    }
}