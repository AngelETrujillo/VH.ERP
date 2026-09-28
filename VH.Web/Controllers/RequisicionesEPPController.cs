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

        /// <summary>Identificador del usuario conectado, tal como lo ve el API.</summary>
        private string? UsuarioActual() =>
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        private void SetAuthHeader()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // GET: RequisicionesEPP
        public async Task<IActionResult> Index(
            string? filtro,
            int pagina = 1, int tamano = ConsultaPaginada.TamanoPorOmision, string? buscar = null)
        {
            SetAuthHeader();

            var consulta = new ConsultaPaginada { Pagina = pagina, Tamano = tamano, Buscar = buscar };
            ViewBag.FiltroActual = filtro;

            try
            {
                var partes = new List<string> { $"pagina={consulta.Pagina}", $"tamano={consulta.Tamano}" };
                if (consulta.HayBusqueda) partes.Add($"buscar={Uri.EscapeDataString(consulta.TextoLimpio!)}");
                if (!string.IsNullOrWhiteSpace(filtro)) partes.Add($"filtro={Uri.EscapeDataString(filtro)}");

                var url = "api/requisicionesepp/paginado?" + string.Join("&", partes);

                var response = await _httpClient.GetAsync(url);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                // Sin permiso para ver todas, se cae a las propias. Era el
                // comportamiento anterior y se conserva.
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    ViewBag.FiltroActual = "mis";
                    var propias = partes.Where(p => !p.StartsWith("filtro=")).ToList();
                    propias.Add("filtro=mis");
                    response = await _httpClient.GetAsync(
                        "api/requisicionesepp/paginado?" + string.Join("&", propias));
                }

                if (response.IsSuccessStatusCode)
                {
                    var pag = await response.Content
                        .ReadFromJsonAsync<ResultadoPaginado<RequisicionEPPResponseDto>>();

                    return View(pag ?? ResultadoPaginado<RequisicionEPPResponseDto>.Ninguno(consulta));
                }

                ViewBag.ErrorMessage = "Error al cargar las requisiciones";
                return View(ResultadoPaginado<RequisicionEPPResponseDto>.Ninguno(consulta));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar requisiciones");
                ViewBag.ErrorMessage = "Error al cargar las requisiciones: " + ex.Message;
                return View(ResultadoPaginado<RequisicionEPPResponseDto>.Ninguno(consulta));
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
        /// <summary>
        /// Pantalla de elección. Pedir EPP para una persona, cargar consumible a una
        /// obra y prestar una herramienta son tres actos distintos, con datos
        /// distintos; un solo formulario con campos que a veces aplican obliga al
        /// almacenista a saber cuáles ignorar.
        ///
        /// Detrás de las tres va el mismo documento, la misma autorización y el
        /// mismo apartado: lo que se separa es la pantalla, no el circuito.
        /// </summary>
        public IActionResult Create() => View();

        // GET: RequisicionesEPP/CrearEPP
        [RequierePermiso("REQUISICIONES_EPP", "crear")]
        public async Task<IActionResult> CrearEPP()
        {
            SetAuthHeader();
            await CargarListasEnViewBag(TipoMaterial.EPP);
            return View();
        }

        // GET: RequisicionesEPP/CrearConsumible
        [RequierePermiso("REQUISICIONES_EPP", "crear")]
        public async Task<IActionResult> CrearConsumible()
        {
            SetAuthHeader();
            await CargarListasEnViewBag(TipoMaterial.Consumible);
            await CargarObrasEnViewBag();
            return View();
        }

        // GET: RequisicionesEPP/CrearHerramienta
        [RequierePermiso("REQUISICIONES_EPP", "crear")]
        public async Task<IActionResult> CrearHerramienta()
        {
            SetAuthHeader();
            await CargarListasEnViewBag(TipoMaterial.Herramienta);
            return View();
        }

        // POST: RequisicionesEPP/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("REQUISICIONES_EPP", "crear")]
        public async Task<IActionResult> Create(RequisicionEPPRequestDto dto, TipoMaterial tipo = TipoMaterial.EPP)
        {
            // A qué pantalla volver si algo falla: la que el almacenista tenía
            // abierta, no una genérica que le pida otra vez lo que ya escribió.
            var vista = tipo switch
            {
                TipoMaterial.Consumible => nameof(CrearConsumible),
                TipoMaterial.Herramienta => nameof(CrearHerramienta),
                _ => nameof(CrearEPP)
            };

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

            await CargarListasEnViewBag(tipo);
            if (tipo == TipoMaterial.Consumible) await CargarObrasEnViewBag();

            return View(vista, dto);
        }

        /// <summary>
        /// Las partidas de una obra, para el selector dependiente de la pantalla de
        /// consumibles. Devuelve JSON: es la pantalla la que pregunta al elegir la
        /// obra, en vez de traer el presupuesto completo de todas por adelantado.
        /// </summary>
        [RequierePermiso("REQUISICIONES_EPP", "crear")]
        public async Task<IActionResult> PartidasDeObra(int idProyecto)
        {
            SetAuthHeader();

            if (idProyecto <= 0) return Json(Array.Empty<object>());

            try
            {
                var response = await _httpClient.GetAsync($"api/proyectos/{idProyecto}/partidas");
                if (!response.IsSuccessStatusCode) return Json(Array.Empty<object>());

                var partidas = await response.Content
                    .ReadFromJsonAsync<IEnumerable<ConceptoPartidaResponseDto>>();

                return Json(partidas?
                    .OrderBy(p => p.Descripcion)
                    .Select(p => new
                    {
                        v = p.IdPartida.ToString(),
                        t = $"{p.Descripcion} ({p.CantidadEstimada:0.##} {p.AbreviaturaUnidadMedida})"
                    })
                    .ToList() ?? new());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar las partidas de la obra {Id}", idProyecto);
                return Json(Array.Empty<object>());
            }
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

            // Nadie autoriza lo que él mismo pidió. El API ya lo rechaza, pero
            // dejar los botones a la vista hace que el usuario marque renglones,
            // confirme y sólo entonces se entere de que no podía: más vale decirlo
            // antes y no ofrecer el botón.
            ViewBag.EsSolicitante = !string.IsNullOrEmpty(requisicion.IdUsuarioSolicita)
                                    && requisicion.IdUsuarioSolicita == UsuarioActual();

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
                // Un documento de puro consumible no tiene a quién entregarle, pero
                // sí tiene material por salir. Antes caía en el mensaje de abajo y
                // se quedaba encerrado: la única puerta pedía un trabajador.
                if (requisicion.TieneConsumoDeObraPorSurtir)
                    return RedirectToAction(nameof(EntregarObra), new { id });

                // Distinguir por qué no hay nada que entregar: no es lo mismo un documento
                // ya surtido que uno cuyo material todavía no llega.
                var esperando = requisicion.Detalles.Count(d =>
                    d.EstadoRenglon == EstadoRenglonRequisicion.PorComprar ||
                    d.EstadoRenglon == EstadoRenglonRequisicion.EnOrdenCompra);

                TempData["Error"] = esperando > 0
                    ? $"Todavía no hay material listo para entregar: {esperando} renglón(es) esperan compra o recepción."
                    : "Esta requisición ya no tiene material por surtir a trabajadores.";
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

                TempData["Error"] = ExtraerMensaje(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al entregar requisición");
                TempData["Error"] = "Error al procesar la entrega";
            }

            return RedirectToAction(nameof(Entregar), new { id });
        }

        // GET: RequisicionesEPP/EntregarObra/5
        // Despacha lo que se carga a la obra. Sin firma y de una sola vez: no hay
        // trabajadores entre los cuales repartir el documento.
        [RequierePermiso("REQUISICIONES_EPP", "editar")]
        public async Task<IActionResult> EntregarObra(int id)
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

            if (!requisicion.TieneConsumoDeObraPorSurtir)
            {
                // Si lo que queda va a personas, la puerta es la otra.
                if (requisicion.EmpleadosPorSurtir.Count > 0)
                    return RedirectToAction(nameof(Entregar), new { id });

                var esperando = requisicion.Detalles.Count(d =>
                    d.EstadoRenglon == EstadoRenglonRequisicion.PorComprar ||
                    d.EstadoRenglon == EstadoRenglonRequisicion.EnOrdenCompra);

                TempData["Error"] = esperando > 0
                    ? $"Todavía no hay material listo para cargar a la obra: {esperando} renglón(es) esperan compra o recepción."
                    : "Esta requisición ya no tiene consumo de obra por despachar.";
                return RedirectToAction(nameof(Details), new { id });
            }

            await CargarLotesDisponibles(requisicion);

            return View(requisicion);
        }

        // POST: RequisicionesEPP/EntregarObra/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("REQUISICIONES_EPP", "editar")]
        public async Task<IActionResult> EntregarObra(int id, EntregarAObraRequestDto dto)
        {
            SetAuthHeader();
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/requisicionesepp/{id}/despachar-obra", dto);
                if (response.IsSuccessStatusCode)
                {
                    // Puede quedar material para trabajadores en el mismo documento:
                    // no es lo normal, pero nada lo impide.
                    var actual = await _httpClient.GetAsync($"api/requisicionesepp/{id}");
                    if (actual.IsSuccessStatusCode)
                    {
                        var req = await actual.Content.ReadFromJsonAsync<RequisicionEPPResponseDto>();
                        if (req != null && req.EmpleadosPorSurtir.Count > 0)
                        {
                            TempData["Mensaje"] = "Consumo cargado a la obra. Quedan trabajadores por recibir su material.";
                            return RedirectToAction(nameof(Entregar), new { id });
                        }
                    }

                    TempData["Mensaje"] = "Consumo cargado a la obra.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                TempData["Error"] = ExtraerMensaje(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar consumo a la obra");
                TempData["Error"] = "Error al procesar el despacho";
            }

            return RedirectToAction(nameof(EntregarObra), new { id });
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
                    TempData["Mensaje"] = "Requisición cancelada. Si tenía material apartado, volvió a quedar disponible.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // El motivo real, por ejemplo que sólo el solicitante puede cancelar,
                // viene en el cuerpo de la respuesta: se muestra como texto, no como JSON.
                TempData["Error"] = ExtraerMensaje(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar requisición");
                TempData["Error"] = "Error al cancelar la requisición";
            }

            // A la ficha y no al listado: ahí se muestran los mensajes y se ve cómo
            // quedó cada renglón.
            return RedirectToAction(nameof(Details), new { id });
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

        /// <summary>
        /// Carga las listas de la pantalla. El <paramref name="tipo"/> acota los
        /// materiales: en la pantalla de herramientas no tiene por qué aparecer un
        /// casco, y ofrecerlo sólo invita a equivocarse.
        /// </summary>
        private async Task CargarListasEnViewBag(TipoMaterial? tipo = null)
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

            // Materiales, acotados al tipo de la pantalla
            var urlMateriales = tipo.HasValue
                ? $"api/materiales?tipo={(int)tipo.Value}"
                : "api/materiales";
            var matResponse = await _httpClient.GetAsync(urlMateriales);
            if (matResponse.IsSuccessStatusCode)
            {
                var materiales = await matResponse.Content.ReadFromJsonAsync<IEnumerable<MaterialResponseDto>>();
                ViewBag.Materiales = materiales?.Where(m => m.Activo).Select(m => new SelectListItem
                {
                    Value = m.IdMaterial.ToString(),
                    Text = $"{m.Nombre} ({m.AbreviaturaUnidadMedida})"
                }).ToList() ?? new List<SelectListItem>();

                // Cuáles piden talla: la pantalla habilita el campo sólo en ésos,
                // en vez de mostrarlo siempre y confiar en que nadie lo llene.
                ViewBag.MaterialesConTalla = materiales?
                    .Where(m => m.Activo && m.RequiereTalla)
                    .Select(m => m.IdMaterial)
                    .ToList() ?? new List<int>();
            }
        }

        /// <summary>
        /// Obras activas, para la pantalla de consumibles. Las partidas de cada una
        /// se piden al elegirla: cargarlas todas de golpe sería traer el presupuesto
        /// entero para usar tres renglones.
        /// </summary>
        private async Task CargarObrasEnViewBag()
        {
            var response = await _httpClient.GetAsync("api/proyectos");
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Obras = new List<SelectListItem>();
                return;
            }

            var obras = await response.Content.ReadFromJsonAsync<IEnumerable<ProyectoResponseDto>>();
            ViewBag.Obras = obras?
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .Select(p => new SelectListItem { Value = p.IdProyecto.ToString(), Text = p.Nombre })
                .ToList() ?? new List<SelectListItem>();
        }

        /// <summary>
        /// Los lotes con existencia de cada material, ya ordenados como se van a
        /// consumir: primero lo que caduca, y a igualdad lo más antiguo.
        ///
        /// Van completos y no como lista de opciones porque la pantalla necesita
        /// sumarlos y anticipar de cuáles va a salir el material.
        /// </summary>
        private async Task CargarLotesDisponibles(RequisicionEPPResponseDto requisicion)
        {
            var lotesDict = new Dictionary<int, List<CompraEPPSimpleDto>>();

            foreach (var detalle in requisicion.Detalles)
            {
                if (lotesDict.ContainsKey(detalle.IdMaterial)) continue;

                try
                {
                    var response = await _httpClient.GetAsync(
                        $"api/comprasepp/lotes-disponibles?idMaterial={detalle.IdMaterial}&idAlmacen={requisicion.IdAlmacen}");

                    var lotes = response.IsSuccessStatusCode
                        ? await response.Content.ReadFromJsonAsync<List<CompraEPPSimpleDto>>()
                        : null;

                    lotesDict[detalle.IdMaterial] = lotes ?? new List<CompraEPPSimpleDto>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al cargar lotes del material {Id}", detalle.IdMaterial);
                    lotesDict[detalle.IdMaterial] = new List<CompraEPPSimpleDto>();
                }
            }

            ViewBag.LotesDisponibles = lotesDict;
        }
    }
}