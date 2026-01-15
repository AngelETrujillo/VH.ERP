using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VH.Services.DTOs;

namespace VH.Web.Controllers
{
    public class EntregasEPPController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EntregasEPPController> _logger;

        public EntregasEPPController(IHttpClientFactory httpClientFactory, ILogger<EntregasEPPController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
            _logger = logger;
        }

        // GET: EntregasEPP
        public async Task<IActionResult> Index(int? idEmpleado)
        {
            try
            {
                var url = "api/entregasepp";
                if (idEmpleado.HasValue)
                {
                    url += $"?idEmpleado={idEmpleado}";
                }

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var entregas = await response.Content.ReadFromJsonAsync<IEnumerable<EntregaEPPResponseDto>>();

                // Cargar lista de empleados para filtro
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

        // GET: EntregasEPP/Create
        public async Task<IActionResult> Create()
        {
            await CargarListasEnViewBag();
            return View(new EntregaEPPRequestDto(0, 0, 0, DateTime.Today, 1, "", ""));
        }

        // POST: EntregasEPP/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EntregaEPPRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/entregasepp", dto);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Entrega registrada exitosamente";
                        return RedirectToAction(nameof(Index));
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error: {error}");
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
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var entrega = await _httpClient.GetFromJsonAsync<EntregaEPPResponseDto>($"api/entregasepp/{id}");
                if (entrega == null) return NotFound();

                var dto = new EntregaEPPRequestDto(
                    entrega.IdEmpleado,
                    entrega.IdMaterial,
                    entrega.IdProveedor,
                    entrega.FechaEntrega,
                    entrega.CantidadEntregada,
                    entrega.TallaEntregada,
                    entrega.Observaciones
                );

                await CargarListasEnViewBag();
                ViewBag.Id = id;
                return View(dto);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: EntregasEPP/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EntregaEPPRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PutAsJsonAsync($"api/entregasepp/{id}", dto);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    ModelState.AddModelError("", "Error al actualizar");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al actualizar entrega EPP");
                    ModelState.AddModelError("", "Error al actualizar");
                }
            }
            await CargarListasEnViewBag();
            ViewBag.Id = id;
            return View(dto);
        }

        // GET: EntregasEPP/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var entrega = await _httpClient.GetFromJsonAsync<EntregaEPPResponseDto>($"api/entregasepp/{id}");
                if (entrega == null) return NotFound();
                return View(entrega);
            }
            catch
            {
                return NotFound();
            }
        }

        // GET: EntregasEPP/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var entrega = await _httpClient.GetFromJsonAsync<EntregaEPPResponseDto>($"api/entregasepp/{id}");
                if (entrega == null) return NotFound();
                return View(entrega);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: EntregasEPP/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/entregasepp/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = "No se pudo eliminar la entrega";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar entrega EPP");
                TempData["ErrorMessage"] = "Error al eliminar";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarListasEnViewBag()
        {
            try
            {
                var empleados = await _httpClient.GetFromJsonAsync<IEnumerable<EmpleadoResponseDto>>("api/empleados");
                ViewBag.Empleados = empleados?.Where(e => e.Activo).Select(e => new SelectListItem
                {
                    Value = e.IdEmpleado.ToString(),
                    Text = $"{e.Nombre} {e.ApellidoPaterno} - {e.NumeroNomina}"
                }).ToList() ?? new List<SelectListItem>();

                var materiales = await _httpClient.GetFromJsonAsync<IEnumerable<MaterialEPPResponseDto>>("api/materiales");
                ViewBag.Materiales = materiales?.Where(m => m.Activo).Select(m => new SelectListItem
                {
                    Value = m.IdMaterial.ToString(),
                    Text = m.Nombre
                }).ToList() ?? new List<SelectListItem>();

                var proveedores = await _httpClient.GetFromJsonAsync<IEnumerable<ProveedorResponseDto>>("api/proveedores");
                ViewBag.Proveedores = proveedores?.Where(p => p.Activo).Select(p => new SelectListItem
                {
                    Value = p.IdProveedor.ToString(),
                    Text = p.Nombre
                }).ToList() ?? new List<SelectListItem>();
            }
            catch
            {
                ViewBag.Empleados = new List<SelectListItem>();
                ViewBag.Materiales = new List<SelectListItem>();
                ViewBag.Proveedores = new List<SelectListItem>();
            }
        }

        private async Task CargarEmpleadosEnViewBag()
        {
            try
            {
                var empleados = await _httpClient.GetFromJsonAsync<IEnumerable<EmpleadoResponseDto>>("api/empleados");
                ViewBag.EmpleadosFiltro = empleados?.Select(e => new SelectListItem
                {
                    Value = e.IdEmpleado.ToString(),
                    Text = $"{e.Nombre} {e.ApellidoPaterno}"
                }).ToList() ?? new List<SelectListItem>();
            }
            catch
            {
                ViewBag.EmpleadosFiltro = new List<SelectListItem>();
            }
        }
    }
}