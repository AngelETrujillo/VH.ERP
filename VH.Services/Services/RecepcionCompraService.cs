using VH.Services.DTOs;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    public class RecepcionCompraService : IRecepcionCompraService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICompraEPPService _compraService;
        private readonly IInventarioService _inventarioService;

        private const string IncludeRecepcion =
            "OrdenCompra.Proveedor,Almacen.Proyecto,UsuarioRecibe," +
            "Detalles.Material.UnidadMedida,Detalles.OrdenCompraDetalle";

        public RecepcionCompraService(
            IUnitOfWork unitOfWork,
            ICompraEPPService compraService,
            IInventarioService inventarioService)
        {
            _unitOfWork = unitOfWork;
            _compraService = compraService;
            _inventarioService = inventarioService;
        }

        // ===== BANDEJA =====

        public async Task<IEnumerable<OrdenPorRecibirDto>> GetPorRecibirAsync(int? idAlmacen = null)
        {
            // Una orden puede repartirse entre varias obras; la bandeja lista una
            // entrada por almacén, porque cada camión llega a una bodega distinta
            // y se recibe por separado.
            var renglones = (await _unitOfWork.OrdenesCompraDetalle.FindAsync(
                    d => d.CantidadRecibida < d.CantidadPedida,
                    includeProperties: "OrdenCompra.Proveedor,Material,AlmacenDestino.Proyecto,Coberturas"))
                .Where(d => d.OrdenCompra != null
                            && d.OrdenCompra.Estado != EstadoOrdenCompra.Cancelada
                            && d.OrdenCompra.Estado != EstadoOrdenCompra.Recibida)
                .Where(d => !idAlmacen.HasValue || d.IdAlmacenDestino == idAlmacen.Value)
                .ToList();

            var bandeja = renglones
                .GroupBy(d => new { d.IdOrdenCompra, d.IdAlmacenDestino })
                .Select(g =>
                {
                    var primero = g.First();
                    var orden = primero.OrdenCompra!;

                    return new OrdenPorRecibirDto
                    {
                        IdOrdenCompra = g.Key.IdOrdenCompra,
                        Folio = orden.Folio,
                        NombreProveedor = orden.Proveedor?.Nombre ?? "",
                        FechaEmision = orden.FechaEmision,
                        FechaEntregaEstimada = orden.FechaEntregaEstimada,

                        IdAlmacen = g.Key.IdAlmacenDestino,
                        NombreAlmacen = primero.AlmacenDestino?.Nombre ?? "",
                        NombreProyecto = primero.AlmacenDestino?.Proyecto?.Nombre ?? "",

                        RenglonesPendientes = g.Count(),
                        PiezasPendientes = g.Sum(d => d.EnTransito),
                        ImportePendiente = g.Sum(d => d.EnTransito * d.PrecioUnitarioPactado),
                        PersonasEsperando = g.Sum(d => d.Coberturas?.Count ?? 0),
                        ResumenMateriales = string.Join(", ",
                            g.Take(3).Select(d => d.Material?.Nombre ?? $"Material {d.IdMaterial}"))
                            + (g.Count() > 3 ? $" y {g.Count() - 3} más" : ""),
                        Parcial = g.Any(d => d.CantidadRecibida > 0)
                    };
                })
                .ToList();

            // Lo más retrasado primero; a igualdad, lo que lleva más tiempo pedido.
            return bandeja
                .OrderByDescending(o => o.DiasRetraso)
                .ThenByDescending(o => o.DiasDesdeEmision)
                .ThenBy(o => o.Folio)
                .ToList();
        }

        public async Task<PreparacionRecepcionDto?> GetPreparacionAsync(int idOrdenCompra, int idAlmacen)
        {
            var orden = await _unitOfWork.OrdenesCompra.GetByIdAsync(
                idOrdenCompra,
                includeProperties: "Proveedor,Detalles.Material.UnidadMedida,Detalles.AlmacenDestino.Proyecto");

            if (orden == null) return null;

            var renglonesAlmacen = orden.Detalles
                .Where(d => d.IdAlmacenDestino == idAlmacen)
                .OrderBy(d => d.IdOrdenCompraDetalle)
                .ToList();

            if (renglonesAlmacen.Count == 0) return null;

            var almacen = renglonesAlmacen.First().AlmacenDestino;

            var preparacion = new PreparacionRecepcionDto
            {
                IdOrdenCompra = orden.IdOrdenCompra,
                Folio = orden.Folio,
                IdProveedor = orden.IdProveedor,
                NombreProveedor = orden.Proveedor?.Nombre ?? "",
                FechaEmision = orden.FechaEmision,
                FechaEntregaEstimada = orden.FechaEntregaEstimada,
                IdAlmacen = idAlmacen,
                NombreAlmacen = almacen?.Nombre ?? "",
                NombreProyecto = almacen?.Proyecto?.Nombre ?? ""
            };

            var idsRenglon = renglonesAlmacen.Select(d => d.IdOrdenCompraDetalle).ToList();

            var coberturas = (await _unitOfWork.RequisicionesCobertura.FindAsync(
                    c => c.IdOrdenCompraDetalle != null && idsRenglon.Contains(c.IdOrdenCompraDetalle.Value),
                    includeProperties: "RequisicionDetalle.Requisicion,RequisicionDetalle.EmpleadoDestino"))
                .Where(c => c.RequisicionDetalle != null)
                .ToList();

            foreach (var renglon in renglonesAlmacen)
            {
                var dto = new PreparacionRenglonDto
                {
                    IdOrdenCompraDetalle = renglon.IdOrdenCompraDetalle,
                    IdMaterial = renglon.IdMaterial,
                    NombreMaterial = renglon.Material?.Nombre ?? $"Material {renglon.IdMaterial}",
                    UnidadMedida = renglon.Material?.UnidadMedida?.Abreviatura ?? "",
                    RequiereTalla = renglon.Material?.RequiereTalla ?? false,
                    ControlaCaducidad = renglon.Material?.ControlaCaducidad ?? false,
                    CantidadPedida = renglon.CantidadPedida,
                    CantidadRecibida = renglon.CantidadRecibida,
                    PrecioUnitarioPactado = renglon.PrecioUnitarioPactado
                };

                var delRenglon = coberturas
                    .Where(c => c.IdOrdenCompraDetalle == renglon.IdOrdenCompraDetalle)
                    .ToList();

                // Quién espera esto, del más antiguo al más nuevo: es el mismo orden
                // en que se les va a cubrir cuando llegue el material.
                var enEspera = delRenglon
                    .Where(c => c.RequisicionDetalle!.EstadoRenglon == EstadoRenglonRequisicion.EnOrdenCompra)
                    .OrderBy(c => c.RequisicionDetalle!.Requisicion?.FechaSolicitud ?? DateTime.MaxValue)
                    .ThenBy(c => c.IdRequisicionDetalle)
                    .ToList();

                // Con lo que falta por llegar se cuenta también lo que ya llegó y
                // todavía no le tocó a nadie, porque una entrega anterior no
                // alcanzó a cubrir un renglón completo.
                var repartido = delRenglon
                    .Where(c => c.RequisicionDetalle!.EstadoRenglon is EstadoRenglonRequisicion.Recibido
                                                                    or EstadoRenglonRequisicion.Surtido)
                    .Sum(c => c.Cantidad);

                var alcanza = dto.Pendiente + Math.Max(0, renglon.CantidadRecibida - repartido);

                foreach (var cobertura in enEspera)
                {
                    var detalle = cobertura.RequisicionDetalle!;
                    var cubierto = alcanza >= cobertura.Cantidad;
                    if (cubierto) alcanza -= cobertura.Cantidad;

                    dto.Esperan.Add(new EsperandoDto
                    {
                        IdRequisicionDetalle = detalle.IdRequisicionDetalle,
                        IdRequisicion = detalle.IdRequisicion,
                        NumeroRequisicion = detalle.Requisicion?.NumeroRequisicion ?? "",
                        Cantidad = cobertura.Cantidad,
                        IdEmpleadoDestino = detalle.IdEmpleadoDestino,
                        NombreEmpleado = detalle.EmpleadoDestino?.NombreCompleto ?? "",
                        FechaSolicitud = detalle.Requisicion?.FechaSolicitud ?? DateTime.Now,
                        CubiertoAhora = cubierto
                    });
                }

                preparacion.Renglones.Add(dto);
            }

            return preparacion;
        }

        // ===== CONSULTA =====

        public async Task<IEnumerable<RecepcionCompra>> GetRecepcionesAsync(
            int? idOrdenCompra = null, int? idAlmacen = null)
        {
            if (idOrdenCompra.HasValue || idAlmacen.HasValue)
            {
                return await _unitOfWork.RecepcionesCompra.FindAsync(
                    r => (!idOrdenCompra.HasValue || r.IdOrdenCompra == idOrdenCompra.Value) &&
                         (!idAlmacen.HasValue || r.IdAlmacen == idAlmacen.Value),
                    includeProperties: IncludeRecepcion);
            }

            return await _unitOfWork.RecepcionesCompra.GetAllAsync(includeProperties: IncludeRecepcion);
        }

        public async Task<RecepcionCompra?> GetRecepcionByIdAsync(int id)
        {
            return await _unitOfWork.RecepcionesCompra.GetByIdAsync(id, includeProperties: IncludeRecepcion);
        }

        // ===== RECEPCIÓN =====

        public async Task<(RecepcionCompra Recepcion, List<string> Avisos)> RecibirAsync(
            RecibirOrdenRequestDto dto, string userId)
        {
            var orden = await _unitOfWork.OrdenesCompra.GetByIdAsync(
                dto.IdOrdenCompra, includeProperties: "Proveedor,Detalles.Material");

            if (orden == null)
                throw new ArgumentException($"La orden de compra {dto.IdOrdenCompra} no existe.");

            if (orden.Estado == EstadoOrdenCompra.Cancelada)
                throw new InvalidOperationException("La orden está cancelada: no puede recibirse material contra ella.");

            var almacen = await _unitOfWork.Almacenes.GetByIdAsync(dto.IdAlmacen)
                ?? throw new ArgumentException($"El almacén {dto.IdAlmacen} no existe.");

            if (dto.Lineas == null || dto.Lineas.Count == 0)
                throw new InvalidOperationException("Debe recibir al menos un renglón.");

            var repetido = dto.Lineas
                .GroupBy(l => l.IdOrdenCompraDetalle)
                .FirstOrDefault(g => g.Count() > 1);
            if (repetido != null)
                throw new InvalidOperationException(
                    $"El renglón {repetido.Key} viene repetido en la recepción.");

            // Validar todo antes de escribir nada: una recepción a medias deja
            // existencia sin documento y renglones sin cubrir.
            var lineasValidadas = new List<(RecibirLineaRequestDto Linea, OrdenCompraDetalle Renglon)>();

            foreach (var linea in dto.Lineas)
            {
                var renglon = orden.Detalles.FirstOrDefault(
                    d => d.IdOrdenCompraDetalle == linea.IdOrdenCompraDetalle);

                if (renglon == null)
                    throw new ArgumentException(
                        $"El renglón {linea.IdOrdenCompraDetalle} no pertenece a la orden {orden.Folio}.");

                var nombre = renglon.Material?.Nombre ?? $"material {renglon.IdMaterial}";

                if (renglon.IdAlmacenDestino != dto.IdAlmacen)
                    throw new InvalidOperationException(
                        $"'{nombre}' está pedido para otro almacén y no puede recibirse en '{almacen.Nombre}'.");

                if (linea.CantidadRecibida <= 0)
                    throw new ArgumentException($"La cantidad recibida de '{nombre}' debe ser mayor a 0.");

                if (linea.CantidadAceptada < 0)
                    throw new ArgumentException($"La cantidad aceptada de '{nombre}' no puede ser negativa.");

                if (linea.CantidadAceptada > linea.CantidadRecibida)
                    throw new InvalidOperationException(
                        $"No se puede aceptar de '{nombre}' más de lo que llegó. " +
                        $"Llegó: {linea.CantidadRecibida}, se acepta: {linea.CantidadAceptada}.");

                var pendiente = renglon.EnTransito;
                if (pendiente <= 0)
                    throw new InvalidOperationException($"'{nombre}' ya se recibió completo en esta orden.");

                if (linea.CantidadAceptada > pendiente)
                    throw new InvalidOperationException(
                        $"Se está aceptando más de lo pedido de '{nombre}'. " +
                        $"Falta por recibir: {pendiente}, se acepta: {linea.CantidadAceptada}. " +
                        "Si el proveedor mandó de más, corrija la orden antes de recibir.");

                if (linea.CantidadRecibida > linea.CantidadAceptada &&
                    string.IsNullOrWhiteSpace(linea.MotivoRechazo))
                    throw new ArgumentException(
                        $"Se está rechazando material de '{nombre}': debe indicar el motivo.");

                if (linea.PrecioUnitarioReal <= 0)
                    throw new ArgumentException($"El precio real de '{nombre}' debe ser mayor a 0.");

                lineasValidadas.Add((linea, renglon));
            }

            var avisos = new List<string>();
            var transaccionPropia = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var recepcion = new RecepcionCompra
                {
                    Folio = await GenerarFolioAsync(),
                    IdOrdenCompra = orden.IdOrdenCompra,
                    IdAlmacen = dto.IdAlmacen,
                    FechaRecepcion = dto.FechaRecepcion,
                    IdUsuarioRecibe = userId,
                    NumeroFactura = dto.NumeroFactura,
                    UuidCFDI = dto.UuidCFDI,
                    Observaciones = dto.Observaciones
                };

                foreach (var (linea, renglon) in lineasValidadas)
                {
                    recepcion.Detalles.Add(new RecepcionCompraDetalle
                    {
                        IdOrdenCompraDetalle = renglon.IdOrdenCompraDetalle,
                        IdMaterial = renglon.IdMaterial,
                        CantidadRecibida = linea.CantidadRecibida,
                        CantidadAceptada = linea.CantidadAceptada,
                        PrecioUnitarioReal = linea.PrecioUnitarioReal,
                        Talla = linea.Talla,
                        FechaCaducidad = linea.FechaCaducidad,
                        LoteProveedor = linea.LoteProveedor,
                        MotivoRechazo = linea.CantidadRecibida > linea.CantidadAceptada
                            ? linea.MotivoRechazo
                            : null
                    });
                }

                await _unitOfWork.RecepcionesCompra.AddAsync(recepcion);
                await _unitOfWork.CompleteAsync();

                // El lote nace de lo aceptado, al precio realmente pagado. Lo
                // rechazado se cuenta y se documenta, pero no entra al almacén.
                await GenerarCompraAsync(recepcion, orden, dto, lineasValidadas, userId);

                // Avanzar la orden y repartir lo que llegó entre quienes lo esperan.
                foreach (var (linea, renglon) in lineasValidadas)
                {
                    var nombre = renglon.Material?.Nombre ?? $"material {renglon.IdMaterial}";

                    if (linea.PrecioUnitarioReal != renglon.PrecioUnitarioPactado)
                    {
                        var diferencia = linea.PrecioUnitarioReal - renglon.PrecioUnitarioPactado;
                        avisos.Add(
                            $"'{nombre}' se pactó a {renglon.PrecioUnitarioPactado:C} y se pagó a " +
                            $"{linea.PrecioUnitarioReal:C} ({diferencia:+0.00;-0.00} por unidad). " +
                            "El costo del lote es el precio pagado.");
                    }

                    if (linea.CantidadRecibida > linea.CantidadAceptada)
                    {
                        avisos.Add(
                            $"Se rechazaron {linea.CantidadRecibida - linea.CantidadAceptada} de '{nombre}': " +
                            $"{linea.MotivoRechazo}. Siguen contando como pendientes de la orden.");
                    }

                    // Sólo lo aceptado avanza la orden: lo rechazado no llegó, y el
                    // renglón debe seguir esperándolo.
                    renglon.CantidadRecibida += linea.CantidadAceptada;
                    _unitOfWork.OrdenesCompraDetalle.Update(renglon);

                    if (linea.CantidadAceptada > 0)
                        avisos.AddRange(await CubrirRequisicionesAsync(renglon, userId));
                }

                orden.Estado = CalcularEstadoOrden(orden);
                _unitOfWork.OrdenesCompra.Update(orden);

                await _unitOfWork.CompleteAsync();
                if (transaccionPropia) await _unitOfWork.CommitTransactionAsync();

                return (recepcion, avisos);
            }
            catch
            {
                if (transaccionPropia) await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Convierte lo aceptado en lotes de inventario. Se apoya en el servicio de
        /// compras para que la entrada al almacén sea exactamente la misma que la
        /// de una compra directa: mismo lote, mismo kardex, mismas alertas.
        /// </summary>
        private async Task GenerarCompraAsync(
            RecepcionCompra recepcion,
            OrdenCompra orden,
            RecibirOrdenRequestDto dto,
            List<(RecibirLineaRequestDto Linea, OrdenCompraDetalle Renglon)> lineas,
            string userId)
        {
            var conMaterial = recepcion.Detalles.Where(d => d.CantidadAceptada > 0).ToList();

            // Puede llegar un camión entero y rechazarse completo. La recepción
            // queda como constancia, pero no hay nada que meter al almacén.
            if (conMaterial.Count == 0) return;

            var compra = new CompraEPP
            {
                IdProveedor = orden.IdProveedor,
                FechaCompra = dto.FechaRecepcion,
                NumeroDocumento = dto.NumeroFactura ?? recepcion.Folio,
                UuidCFDI = dto.UuidCFDI,
                Iva = dto.Iva,
                Moneda = orden.Moneda,
                Observaciones = $"Recepción {recepcion.Folio} de la orden {orden.Folio}."
            };

            foreach (var detalle in conMaterial)
            {
                compra.Detalles.Add(new CompraEPPDetalle
                {
                    IdMaterial = detalle.IdMaterial,
                    IdAlmacen = recepcion.IdAlmacen,
                    Cantidad = detalle.CantidadAceptada,
                    PrecioUnitario = detalle.PrecioUnitarioReal,
                    Talla = detalle.Talla,
                    FechaCaducidad = detalle.FechaCaducidad
                });
            }

            var (creada, _) = await _compraService.CreateCompraAsync(compra, userId);

            recepcion.IdCompra = creada.IdCompra;
            _unitOfWork.RecepcionesCompra.Update(recepcion);

            // Cada renglón de la recepción apunta al lote que generó: es lo que
            // permite ir del defecto de un proveedor al material que salió de él.
            var lotes = creada.Detalles.ToList();
            for (var i = 0; i < conMaterial.Count && i < lotes.Count; i++)
            {
                conMaterial[i].IdCompraDetalle = lotes[i].IdCompraDetalle;
                _unitOfWork.RecepcionesCompraDetalle.Update(conMaterial[i]);
            }

            await _unitOfWork.CompleteAsync();
        }

        /// <summary>
        /// Reparte lo que ha llegado de este renglón entre las requisiciones que
        /// venía a cubrir, del más antiguo al más nuevo. A cada una se le aparta su
        /// cantidad y queda en <see cref="EstadoRenglonRequisicion.Recibido"/>,
        /// lista para surtirse y firmarse.
        ///
        /// Mira lo acumulado del renglón, no lo que trajo este camión: si se
        /// pidieron cuatro pares, llegaron tres y después el cuarto, quien los
        /// esperaba se cubre con la segunda entrega. Contando sólo el último
        /// evento, ese renglón se quedaba esperando para siempre con el material
        /// ya en la bodega.
        /// </summary>
        private async Task<List<string>> CubrirRequisicionesAsync(
            OrdenCompraDetalle renglon, string userId)
        {
            var avisos = new List<string>();

            var todas = (await _unitOfWork.RequisicionesCobertura.FindAsync(
                    c => c.IdOrdenCompraDetalle == renglon.IdOrdenCompraDetalle,
                    includeProperties: "RequisicionDetalle.Requisicion,RequisicionDetalle.EmpleadoDestino"))
                .Where(c => c.RequisicionDetalle != null)
                .ToList();

            // Lo que ya se le entregó a alguien de este renglón no vuelve a repartirse.
            var repartido = todas
                .Where(c => c.RequisicionDetalle!.EstadoRenglon is EstadoRenglonRequisicion.Recibido
                                                                or EstadoRenglonRequisicion.Surtido)
                .Sum(c => c.Cantidad);

            var coberturas = todas
                .Where(c => c.RequisicionDetalle!.EstadoRenglon == EstadoRenglonRequisicion.EnOrdenCompra)
                .OrderBy(c => c.RequisicionDetalle!.Requisicion?.FechaSolicitud ?? DateTime.MaxValue)
                .ThenBy(c => c.IdRequisicionDetalle)
                .ToList();

            var disponible = renglon.CantidadRecibida - repartido;

            foreach (var cobertura in coberturas)
            {
                // Una entrega parcial no cubre a medias a nadie: se cubre completo
                // a quien lleva más tiempo esperando, y el resto sigue en espera.
                if (disponible < cobertura.Cantidad) break;

                var detalle = cobertura.RequisicionDetalle!;
                var requisicion = detalle.Requisicion;
                var idAlmacen = requisicion?.IdAlmacen ?? renglon.IdAlmacenDestino;

                var apartado = await _inventarioService.ReservarAsync(
                    detalle.IdMaterial, idAlmacen, cobertura.Cantidad,
                    userId, detalle.IdRequisicion, requisicion?.NumeroRequisicion);

                if (!apartado)
                {
                    // Se acaba de sumar la existencia, así que esto sólo ocurre si
                    // alguien más se la llevó en el intervalo. El renglón se queda
                    // en la orden y se avisa, en vez de marcarlo listo en falso.
                    avisos.Add(
                        $"No se pudo apartar el material de {detalle.EmpleadoDestino?.NombreCompleto ?? "un renglón"} " +
                        $"({requisicion?.NumeroRequisicion}): otra operación tomó la existencia. Revise el inventario.");
                    continue;
                }

                detalle.EstadoRenglon = EstadoRenglonRequisicion.Recibido;
                _unitOfWork.RequisicionesEPPDetalle.Update(detalle);

                disponible -= cobertura.Cantidad;

                var quien = detalle.EmpleadoDestino?.NombreCompleto;
                avisos.Add(string.IsNullOrWhiteSpace(quien)
                    ? $"Ya se puede surtir {cobertura.Cantidad} de la requisición {requisicion?.NumeroRequisicion}."
                    : $"Ya se puede surtir a {quien}: {cobertura.Cantidad} de la requisición {requisicion?.NumeroRequisicion}.");
            }

            return avisos;
        }

        private static EstadoOrdenCompra CalcularEstadoOrden(OrdenCompra orden)
        {
            if (orden.Detalles.All(d => d.CantidadRecibida >= d.CantidadPedida))
                return EstadoOrdenCompra.Recibida;

            if (orden.Detalles.Any(d => d.CantidadRecibida > 0))
                return EstadoOrdenCompra.ParcialmenteRecibida;

            return EstadoOrdenCompra.Emitida;
        }

        public async Task<string> GenerarFolioAsync()
        {
            var anio = DateTime.Now.Year;
            var prefijo = $"REC-{anio}-";

            var recepciones = await _unitOfWork.RecepcionesCompra.FindAsync(
                r => r.Folio.StartsWith(prefijo));

            var ultimo = 0;
            if (recepciones.Any())
            {
                ultimo = recepciones
                    .Select(r => int.TryParse(r.Folio.Replace(prefijo, ""), out var n) ? n : 0)
                    .Max();
            }

            return $"{prefijo}{(ultimo + 1):D4}";
        }
    }
}
