using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VH.Services.DTOs;

namespace VH.Web.Controllers
{
    public class AlmacenesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AlmacenesController> _logger;

        public AlmacenesController(IHttpClientFactory httpClientFactory, ILogger<AlmacenesController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
            _logger = logger;
        }

        // GET: Almacenes
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/almacenes");
                response.EnsureSuccessStatusCode();
                var almacenes = await response.Content.ReadFromJsonAsync<IEnumerable<AlmacenResponseDto>>();
                return View(almacenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar almacenes");
                ViewBag.ErrorMessage = "Error al cargar los almacenes";
                return View(new List<AlmacenResponseDto>());
            }
        }

        // GET: Almacenes/Create
        public async Task<IActionResult> Create()
        {
            await CargarProyectosEnViewBag();
            return View();
        }

        // POST: Almacenes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AlmacenRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/almacenes", dto);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error: {error}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear almacén");
                    ModelState.AddModelError("", "Error al crear el almacén");
                }
            }
            await CargarProyectosEnViewBag();
            return View(dto);
        }

        // GET: Almacenes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var almacen = await _httpClient.GetFromJsonAsync<AlmacenResponseDto>($"api/almacenes/{id}");
                if (almacen == null) return NotFound();

                var dto = new AlmacenRequestDto(
                    almacen.Nombre,
                    almacen.Descripcion,
                    almacen.Domicilio,
                    almacen.TipoUbicacion,
                    almacen.Activo,
                    almacen.IdProyecto
                );

                await CargarProyectosEnViewBag();
                ViewBag.Id = id;
                return View(dto);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Almacenes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AlmacenRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PutAsJsonAsync($"api/almacenes/{id}", dto);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    ModelState.AddModelError("", "Error al actualizar");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al actualizar almacén");
                    ModelState.AddModelError("", "Error al actualizar");
                }
            }
            await CargarProyectosEnViewBag();
            ViewBag.Id = id;
            return View(dto);
        }

        // GET: Almacenes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var almacen = await _httpClient.GetFromJsonAsync<AlmacenResponseDto>($"api/almacenes/{id}");
                if (almacen == null) return NotFound();
                return View(almacen);
            }
            catch
            {
                return NotFound();
            }
        }

        // GET: Almacenes/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var almacen = await _httpClient.GetFromJsonAsync<AlmacenResponseDto>($"api/almacenes/{id}");
                if (almacen == null) return NotFound();
                return View(almacen);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Almacenes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/almacenes/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"No se pudo eliminar: {error}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar almacén");
                TempData["ErrorMessage"] = "Error al eliminar el almacén";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarProyectosEnViewBag()
        {
            try
            {
                var proyectos = await _httpClient.GetFromJsonAsync<IEnumerable<ProyectoResponseDto>>("api/proyectos");
                ViewBag.Proyectos = proyectos?.Select(p => new SelectListItem
                {
                    Value = p.IdProyecto.ToString(),
                    Text = p.Nombre
                }).ToList() ?? new List<SelectListItem>();
            }
            catch
            {
                ViewBag.Proyectos = new List<SelectListItem>();
            }
        }
    }
}