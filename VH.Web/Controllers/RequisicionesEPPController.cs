using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VH.Services.DTOs;
using VH.Services.Entities;
using VH.Web.Filters;

namespace VH.Web.Controllers
{
    [Authorize]
    [RequierePermiso("REQUISICIONES_EPP", "ver")]
    public class RequisicionesEPPController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RequisicionesEPPController> _logger;

        public RequisicionesEPPController(IHttpClientFactory httpClientFactory, ILogger<RequisicionesEPPController> logger)
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

        // GET: RequisicionesEPP
        public async Task<IActionResult> Index(string? filtro)
        {
            SetAuthHeader();
            try
            {
                var url = filtro switch
                {
                    "mis" => "api/requisicionesepp/mis-requisiciones",
                    "pendientes-aprobacion" => "api/requisicionesepp/pendientes-aprobacion",
                    "pendientes-entrega" => "api/requisicionesepp/pendientes-entrega",
                    _ => "api/requisicionesepp"
                };

                _logger.LogInformation("Llamando a: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                _logger.LogInformation("Status Code: {StatusCode}", response.StatusCode);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    response = await _httpClient.GetAsync("api/requisicionesepp/mis-requisiciones");
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Response Content: {Content}", content);

                if (response.IsSuccessStatusCode)
                {
                    var requisiciones = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<RequisicionEPPResponseDto>>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    ViewBag.FiltroActual = filtro;
                    return View(requisiciones ?? new List<RequisicionEPPResponseDto>());
                }

                ViewBag.ErrorMessage = "Error al cargar las requisiciones";
                return View(new List<RequisicionEPPResponseDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar requisiciones");
                ViewBag.ErrorMessage = "Error al cargar las requisiciones: " + ex.Message;
                return View(new List<RequisicionEPPResponseDto>());
            }
        }

        // GET: RequisicionesEPP/Details/5
        public async Task<IActionResult> Details(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/requisicionesepp/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var requisicion = await response.Content.ReadFromJsonAsync<RequisicionEPPResponseDto>();
            return View(requisicion);
        }

        // GET: RequisicionesEPP/Create
        [RequierePermiso("REQUISICIONES_EPP", "crear")]
        public async Task<IActionResult> Create()
        {
            SetAuthHeader();
            await CargarListasEnViewBag();
            return View();
        }

        // POST: RequisicionesEPP/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("REQUISICIONES_EPP", "crear")]
        public async Task<IActionResult> Create(RequisicionEPPRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                SetAuthHeader();
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/requisicionesepp", dto);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Mensaje"] = "Requisición creada exitosamente";
                        return RedirectToAction(nameof(Index));
                    }

                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", error);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear requisición");
                    ModelState.AddModelError("", "Error al crear la requisición");
                }
            }

            await CargarListasEnViewBag();
            return View(dto);
        }

        // GET: RequisicionesEPP/Aprobar/5
        [RequierePermiso("REQUISICIONES_EPP", "editar")]
        public async Task<IActionResult> Aprobar(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/requisicionesepp/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var requisicion = await response.Content.ReadFromJsonAsync<RequisicionEPPResponseDto>();

            // La autorización es por renglón: mientras quede alguno sin decidir, la
            // página debe abrir aunque el documento ya tenga renglones autorizados.
            // Antes sólo abría con el documento completo pendiente, y tras la primera
            // decisión el resto de los renglones se quedaba sin forma de resolverse.
            if (requisicion == null || !requisicion.TieneRenglonesPorDecidir)
            {
                TempData["Error"] = "Esta requisición no tiene renglones pendientes de autorizar.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(requisicion);
        }

        // POST: RequisicionesEPP/Aprobar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("REQUISICIONES_EPP", "editar")]
        public async Task<IActionResult> Aprobar(int id, AprobarRequisicionRequestDto dto)
        {
            SetAuthHeader();
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/requisicionesepp/{id}/aprobar", dto);
                if (response.IsSuccessStatusCode)
                {
                    var cuantos = dto.IdsRenglones?.Count ?? 0;
                    var accion = dto.Aprobada ? "autorizado(s)" : "rechazado(s)";
                    TempData["Mensaje"] = cuantos > 0
                        ? $"{cuantos} renglón(es) {accion}."
                        : $"Renglones pendientes {accion}.";

                    // A la ficha y no al listado: ahí se ve cómo quedó cada renglón, y
                    // desde ahí se decide lo que falte.
                    return RedirectToAction(nameof(Details), new { id });
                }

                TempData["Error"] = ExtraerMensaje(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aprobar/rechazar requisición");
                TempData["Error"] = "Error al procesar la solicitud";
            }

            return RedirectToAction(nameof(Aprobar), new { id });
        }

        /// <summary>
        /// El API responde los errores como JSON; al usuario se le muestra sólo el
        /// mensaje, no el cuerpo crudo.
        /// </summary>
        private static string ExtraerMensaje(string cuerpo)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(cuerpo);
                foreach (var nombre in new[] { "mensaje", "message" })
                {
                    if (doc.RootElement.TryGetProperty(nombre, out var valor) &&
                        valor.ValueKind == System.Text.Json.JsonValueKind.String)
                        return valor.GetString() ?? cuerpo;
                }
            }
            catch (System.Text.Json.JsonException)
            {
                // No era JSON: se muestra tal cual.
            }

            return cuerpo;
        }

        // GET: RequisicionesEPP/Entregar/5?idEmpleado=7
        // Se surte de persona en persona: cada trabajador firma lo suyo.
        [RequierePermiso("REQUISICIONES_EPP", "editar")]
        public async Task<IActionResult> Entregar(int id, int? idEmpleado = null)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/requisicionesepp/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var requisicion = await response.Content.ReadFromJsonAsync<RequisicionEPPResponseDto>();
            if (requisicion == null)
                return NotFound();

            if (requisicion.EstadoRequisicion != EstadoRequisicion.Aprobada &&
                requisicion.EstadoRequisicion != EstadoRequisicion.Parcial)
            {
                TempData["Error"] = "Solo se puede surtir material de requisiciones autorizadas";
                return RedirectToAction(nameof(Index));
            }

            var pendientes = requisicion.EmpleadosPorSurtir;
            if (pendientes.Count == 0)
            {
                TempData["Error"] = "Esta requisición ya no tiene material por surtir a trabajadores";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Sin empleado en la URL se atiende al primero que quede por recibir.
            var destino = idEmpleado ?? pendientes.First();
            if (!pendientes.Contains(destino))
            {
                TempData["Error"] = "Ese trabajador no tiene material pendiente en esta requisición";
                return RedirectToAction(nameof(Entregar), new { id });
            }

            ViewBag.IdEmpleadoDestino = destino;
            ViewBag.EmpleadosPendientes = pendientes;

            await CargarLotesDisponibles(requisicion);

            return View(requisicion);
        }

        // POST: RequisicionesEPP/Entregar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("REQUISICIONES_EPP", "editar")]
        public async Task<IActionResult> Entregar(int id, EntregarRequisicionRequestDto dto)
        {
            SetAuthHeader();
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/requisicionesepp/{id}/entregar", dto);
                if (response.IsSuccessStatusCode)
                {
                    // Si quedan trabajadores por recibir, se sigue con el siguiente
                    // en lugar de volver al listado.
                    var actual = await _httpClient.GetAsync($"api/requisicionesepp/{id}");
                    if (actual.IsSuccessStatusCode)
                    {
                        var req = await actual.Content.ReadFromJsonAsync<RequisicionEPPResponseDto>();
                        var pendientes = req?.EmpleadosPorSurtir ?? new List<int>();

                        if (pendientes.Count > 0)
                        {
                            TempData["Mensaje"] = "Entrega firmada. Continúe con el siguiente trabajador.";
                            return RedirectToAction(nameof(Entregar), new { id, idEmpleado = pendientes.First() });
                        }
                    }

                    TempData["Mensaje"] = "Entrega firmada. La requisición quedó surtida por completo.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = error;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al entregar requisición");
                TempData["Error"] = "Error al procesar la entrega";
            }

            return RedirectToAction(nameof(Entregar), new { id });
        }

        // POST: RequisicionesEPP/Cancelar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            SetAuthHeader();
            try
            {
                var response = await _httpClient.PostAsync($"api/requisicionesepp/{id}/cancelar", null);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Requisición cancelada";
                    return RedirectToAction(nameof(Index));
                }

                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = error;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar requisición");
                TempData["Error"] = "Error al cancelar la requisición";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: RequisicionesEPP/SubirFoto
        [HttpPost]
        [RequierePermiso("REQUISICIONES_EPP", "editar")]
        public async Task<IActionResult> SubirFoto(IFormFile foto)
        {
            if (foto == null || foto.Length == 0)
                return BadRequest(new { mensaje = "No se recibió ninguna foto" });

            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "evidencias");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(foto.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await foto.CopyToAsync(stream);
                }

                return Ok(new { ruta = $"/uploads/evidencias/{fileName}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir foto");
                return StatusCode(500, new { mensaje = "Error al subir la foto" });
            }
        }

        // AJAX: Obtener materiales por almacén
        [HttpGet]
        public async Task<IActionResult> GetMateriales()
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync("api/materiales");
            if (!response.IsSuccessStatusCode)
                return Json(new List<object>());

            var materiales = await response.Content.ReadFromJsonAsync<IEnumerable<MaterialResponseDto>>();
            return Json(materiales?.Select(m => new { m.IdMaterial, m.Nombre, UnidadMedida = m.AbreviaturaUnidadMedida }));
        }

        // AJAX: Obtener lotes disponibles
        [HttpGet]
        public async Task<IActionResult> GetLotesDisponibles(int idMaterial, int idAlmacen)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/comprasepp/lotes-disponibles?idMaterial={idMaterial}&idAlmacen={idAlmacen}");
            if (!response.IsSuccessStatusCode)
                return Json(new List<object>());

            var lotes = await response.Content.ReadFromJsonAsync<IEnumerable<CompraEPPSimpleDto>>();
            return Json(lotes?.Select(l => new
            {
                l.IdCompraDetalle,
                l.CantidadDisponible,
                Descripcion = $"Lote #{l.IdCompraDetalle} - {l.NombreProveedor} - Disp: {l.CantidadDisponible}"
            }));
        }

        private async Task CargarListasEnViewBag()
        {
            // Empleados
            var empResponse = await _httpClient.GetAsync("api/empleados");
            if (empResponse.IsSuccessStatusCode)
            {
                var empleados = await empResponse.Content.ReadFromJsonAsync<IEnumerable<EmpleadoResponseDto>>();
                ViewBag.Empleados = empleados?.Select(e => new SelectListItem
                {
                    Value = e.IdEmpleado.ToString(),
                    Text = $"{e.NumeroNomina} - {e.Nombre} {e.ApellidoPaterno}"
                }).ToList() ?? new List<SelectListItem>();
            }

            // Almacenes
            var almResponse = await _httpClient.GetAsync("api/almacenes");
            if (almResponse.IsSuccessStatusCode)
            {
                var almacenes = await almResponse.Content.ReadFromJsonAsync<IEnumerable<AlmacenResponseDto>>();
                ViewBag.Almacenes = almacenes?.Where(a => a.Activo).Select(a => new SelectListItem
                {
                    Value = a.IdAlmacen.ToString(),
                    Text = $"{a.Nombre} ({a.NombreProyecto})"
                }).ToList() ?? new List<SelectListItem>();
            }

            // Materiales
            var matResponse = await _httpClient.GetAsync("api/materiales");
            if (matResponse.IsSuccessStatusCode)
            {
                var materiales = await matResponse.Content.ReadFromJsonAsync<IEnumerable<MaterialResponseDto>>();
                ViewBag.Materiales = materiales?.Where(m => m.Activo).Select(m => new SelectListItem
                {
                    Value = m.IdMaterial.ToString(),
                    Text = $"{m.Nombre} ({m.AbreviaturaUnidadMedida})"
                }).ToList() ?? new List<SelectListItem>();
            }
        }

        private async Task CargarLotesDisponibles(RequisicionEPPResponseDto requisicion)
        {
            var lotesDict = new Dictionary<int, List<SelectListItem>>();

            foreach (var detalle in requisicion.Detalles)
            {
                var response = await _httpClient.GetAsync($"api/comprasepp/lotes-disponibles?idMaterial={detalle.IdMaterial}&idAlmacen={requisicion.IdAlmacen}");
                if (response.IsSuccessStatusCode)
                {
                    var lotes = await response.Content.ReadFromJsonAsync<IEnumerable<CompraEPPSimpleDto>>();
                    lotesDict[detalle.IdMaterial] = lotes?.Select(l => new SelectListItem
                    {
                        Value = l.IdCompraDetalle.ToString(),
                        Text = $"Lote #{l.IdCompraDetalle} - {l.NombreProveedor} - Disponible: {l.CantidadDisponible}"
                    }).ToList() ?? new List<SelectListItem>();
                }
                else
                {
                    lotesDict[detalle.IdMaterial] = new List<SelectListItem>();
                }
            }

            ViewBag.LotesDisponibles = lotesDict;
        }
    }
}