using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VH.Services.DTOs;
using VH.Web.Filters;

namespace VH.Web.Controllers
{
    [Authorize]
    [RequierePermiso("ENTREGAS_EPP", "ver")]
    public class EntregasEPPController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EntregasEPPController> _logger;

        public EntregasEPPController(IHttpClientFactory httpClientFactory, ILogger<EntregasEPPController> logger)
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

        // GET: EntregasEPP
        public async Task<IActionResult> Index(int? idEmpleado)
        {
            SetAuthHeader();
            try
            {
                var url = "api/entregasepp";
                if (idEmpleado.HasValue)
                    url += $"?idEmpleado={idEmpleado}";

                var response = await _httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                response.EnsureSuccessStatusCode();
                var entregas = await response.Content.ReadFromJsonAsync<IEnumerable<EntregaEPPResponseDto>>();

                await CargarEmpleadosEnViewBag();
                ViewBag.FiltroEmpleado = idEmpleado;

                return View(entregas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar entregas EPP");
                ViewBag.ErrorMessage = "Error al cargar las entregas";
                return View(new List<EntregaEPPResponseDto>());
            }
        }

        // GET: EntregasEPP/Details/5
        public async Task<IActionResult> Details(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/entregasepp/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var entrega = await response.Content.ReadFromJsonAsync<EntregaEPPResponseDto>();
            return View(entrega);
        }

        // GET: EntregasEPP/Create
        [RequierePermiso("ENTREGAS_EPP", "crear")]
        public async Task<IActionResult> Create()
        {
            SetAuthHeader();
            await CargarListasEnViewBag();
            return View(new EntregaEPPRequestDto(0, 0, DateTime.Today, 1, "", ""));
        }

        // POST: EntregasEPP/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("ENTREGAS_EPP", "crear")]
        public async Task<IActionResult> Create(EntregaEPPRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                SetAuthHeader();
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/entregasepp", dto);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Mensaje"] = "Entrega registrada exitosamente";
                        return RedirectToAction(nameof(Index));
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", error);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear entrega EPP");
                    ModelState.AddModelError("", "Error al registrar la entrega");
                }
            }
            await CargarListasEnViewBag();
            return View(dto);
        }

        // GET: EntregasEPP/Edit/5
        [RequierePermiso("ENTREGAS_EPP", "editar")]
        public async Task<IActionResult> Edit(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/entregasepp/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var entrega = await response.Content.ReadFromJsonAsync<EntregaEPPResponseDto>();
            var dto = new EntregaEPPRequestDto(
                entrega!.IdEmpleado,
                entrega.IdCompra,
                entrega.FechaEntrega,
                entrega.CantidadEntregada,
                entrega.TallaEntregada,
                entrega.Observaciones
            );

            await CargarListasEnViewBag();
            ViewBag.Id = id;
            return View(dto);
        }

        // POST: EntregasEPP/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("ENTREGAS_EPP", "editar")]
        public async Task<IActionResult> Edit(int id, EntregaEPPRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                SetAuthHeader();
                var response = await _httpClient.PutAsJsonAsync($"api/entregasepp/{id}", dto);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Entrega actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error al actualizar");
            }
            await CargarListasEnViewBag();
            ViewBag.Id = id;
            return View(dto);
        }

        // GET: EntregasEPP/Delete/5
        [RequierePermiso("ENTREGAS_EPP", "eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/entregasepp/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var entrega = await response.Content.ReadFromJsonAsync<EntregaEPPResponseDto>();
            return View(entrega);
        }

        // POST: EntregasEPP/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequierePermiso("ENTREGAS_EPP", "eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.DeleteAsync($"api/entregasepp/{id}");
            if (response.IsSuccessStatusCode)
                TempData["Mensaje"] = "Entrega eliminada";
            else
                TempData["Error"] = "No se pudo eliminar la entrega";

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarListasEnViewBag()
        {
            try
            {
                var empleadosResponse = await _httpClient.GetAsync("api/empleados");
                if (empleadosResponse.IsSuccessStatusCode)
                {
                    var empleados = await empleadosResponse.Content.ReadFromJsonAsync<IEnumerable<EmpleadoResponseDto>>();
                    ViewBag.Empleados = empleados?.Where(e => e.Activo).Select(e => new SelectListItem
                    {
                        Value = e.IdEmpleado.ToString(),
                        Text = $"{e.Nombre} {e.ApellidoPaterno} - {e.NumeroNomina}"
                    }).ToList() ?? new List<SelectListItem>();
                }
                else
                {
                    ViewBag.Empleados = new List<SelectListItem>();
                }

                var materialesResponse = await _httpClient.GetAsync("api/materiales");
                if (materialesResponse.IsSuccessStatusCode)
                {
                    var materiales = await materialesResponse.Content.ReadFromJsonAsync<IEnumerable<MaterialEPPResponseDto>>();
                    ViewBag.Materiales = materiales?.Where(m => m.Activo).Select(m => new SelectListItem
                    {
                        Value = m.IdMaterial.ToString(),
                        Text = m.Nombre
                    }).ToList() ?? new List<SelectListItem>();
                }
                else
                {
                    ViewBag.Materiales = new List<SelectListItem>();
                }

                var almacenesResponse = await _httpClient.GetAsync("api/almacenes");
                if (almacenesResponse.IsSuccessStatusCode)
                {
                    var almacenes = await almacenesResponse.Content.ReadFromJsonAsync<IEnumerable<AlmacenResponseDto>>();
                    ViewBag.Almacenes = almacenes?.Where(a => a.Activo).Select(a => new SelectListItem
                    {
                        Value = a.IdAlmacen.ToString(),
                        Text = a.Nombre
                    }).ToList() ?? new List<SelectListItem>();
                }
                else
                {
                    ViewBag.Almacenes = new List<SelectListItem>();
                }
            }
            catch
            {
                ViewBag.Empleados = new List<SelectListItem>();
                ViewBag.Materiales = new List<SelectListItem>();
                ViewBag.Almacenes = new List<SelectListItem>();
            }
        }

        private async Task CargarEmpleadosEnViewBag()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/empleados");
                if (response.IsSuccessStatusCode)
                {
                    var empleados = await response.Content.ReadFromJsonAsync<IEnumerable<EmpleadoResponseDto>>();
                    ViewBag.EmpleadosFiltro = empleados?.Select(e => new SelectListItem
                    {
                        Value = e.IdEmpleado.ToString(),
                        Text = $"{e.Nombre} {e.ApellidoPaterno}"
                    }).ToList() ?? new List<SelectListItem>();
                }
                else
                {
                    ViewBag.EmpleadosFiltro = new List<SelectListItem>();
                }
            }
            catch
            {
                ViewBag.EmpleadosFiltro = new List<SelectListItem>();
            }
        }
    }
}