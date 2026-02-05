using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VH.Services.DTOs;
using VH.Web.Filters;

namespace VH.Web.Controllers
{
    [Authorize]
    [RequierePermiso("ALMACENES", "ver")]
    public class AlmacenesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AlmacenesController> _logger;

        public AlmacenesController(IHttpClientFactory httpClientFactory, ILogger<AlmacenesController> logger)
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

        // GET: Almacenes
        public async Task<IActionResult> Index()
        {
            SetAuthHeader();
            try
            {
                var response = await _httpClient.GetAsync("api/almacenes");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

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

        // GET: Almacenes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/almacenes/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var almacen = await response.Content.ReadFromJsonAsync<AlmacenResponseDto>();
            return View(almacen);
        }

        // GET: Almacenes/Create
        [RequierePermiso("ALMACENES", "crear")]
        public async Task<IActionResult> Create()
        {
            SetAuthHeader();
            await CargarProyectosEnViewBag();
            return View();
        }

        // POST: Almacenes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("ALMACENES", "crear")]
        public async Task<IActionResult> Create(AlmacenRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                SetAuthHeader();
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/almacenes", dto);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Mensaje"] = "Almacén creado exitosamente";
                        return RedirectToAction(nameof(Index));
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", error);
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
        [RequierePermiso("ALMACENES", "editar")]
        public async Task<IActionResult> Edit(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/almacenes/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var almacen = await response.Content.ReadFromJsonAsync<AlmacenResponseDto>();
            var dto = new AlmacenRequestDto(
                almacen!.Nombre,
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

        // POST: Almacenes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("ALMACENES", "editar")]
        public async Task<IActionResult> Edit(int id, AlmacenRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                SetAuthHeader();
                var response = await _httpClient.PutAsJsonAsync($"api/almacenes/{id}", dto);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Almacén actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error al actualizar");
            }
            await CargarProyectosEnViewBag();
            ViewBag.Id = id;
            return View(dto);
        }

        // GET: Almacenes/Delete/5
        [RequierePermiso("ALMACENES", "eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/almacenes/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var almacen = await response.Content.ReadFromJsonAsync<AlmacenResponseDto>();
            return View(almacen);
        }

        // POST: Almacenes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequierePermiso("ALMACENES", "eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.DeleteAsync($"api/almacenes/{id}");
            if (response.IsSuccessStatusCode)
                TempData["Mensaje"] = "Almacén eliminado";
            else
                TempData["Error"] = "No se pudo eliminar el almacén";

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarProyectosEnViewBag()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/proyectos");
                if (response.IsSuccessStatusCode)
                {
                    var proyectos = await response.Content.ReadFromJsonAsync<IEnumerable<ProyectoResponseDto>>();
                    ViewBag.Proyectos = new SelectList(proyectos, "IdProyecto", "Nombre");
                }
                else
                {
                    ViewBag.Proyectos = new SelectList(new List<ProyectoResponseDto>(), "IdProyecto", "Nombre");
                }
            }
            catch
            {
                ViewBag.Proyectos = new SelectList(new List<ProyectoResponseDto>(), "IdProyecto", "Nombre");
            }
        }
    }
}