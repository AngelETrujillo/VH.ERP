using Microsoft.EntityFrameworkCore;
using VH.Services.DTOs;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    public class RequisicionEPPService : IRequisicionEPPService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEntregaEPPService _entregaService;
        private readonly IInventarioService _inventarioService;
        private readonly ICompraEPPService _compraService;

        /// <summary>
        /// Para listados. Deliberadamente sin Entregas: cada firma es un PNG de
        /// varios KB y traerlas en una consulta de listado la vuelve inservible.
        /// </summary>
        private const string IncludeListado =
            "UsuarioSolicita,Almacen,UsuarioAprueba," +
            "Detalles.Material.UnidadMedida,Detalles.EmpleadoDestino";

        /// <summary>
        /// Para el detalle de un documento. Las firmas no van aquí: se cargan en
        /// una segunda consulta, porque acumular caminos de include en una sola
        /// dispara el tiempo que EF tarda en generar el SQL — con nueve caminos la
        /// misma consulta pasaba de milisegundos a 25 segundos.
        /// </summary>
        private const string IncludeCompleto =
            "UsuarioSolicita,Almacen,UsuarioAprueba," +
            "Detalles.Material.UnidadMedida,Detalles.EmpleadoDestino,Detalles.CompraDetalle";

        public RequisicionEPPService(
            IUnitOfWork unitOfWork,
            IEntregaEPPService entregaService,
            IInventarioService inventarioService,
            ICompraEPPService compraService)
        {
            _unitOfWork = unitOfWork;
            _entregaService = entregaService;
            _inventarioService = inventarioService;
            _compraService = compraService;
        }

        public async Task<IEnumerable<RequisicionEPP>> GetAllAsync()
        {
            return await _unitOfWork.RequisicionesEPP.GetAllAsync(includeProperties: IncludeListado);
        }

        public async Task<IEnumerable<RequisicionEPP>> GetByUsuarioAsync(string userId)
        {
            return await _unitOfWork.RequisicionesEPP.FindAsync(
                r => r.IdUsuarioSolicita == userId,
                includeProperties: IncludeListado);
        }

        public async Task<IEnumerable<RequisicionEPP>> GetByEmpleadoAsync(int idEmpleado)
        {
            // El empleado vive en los renglones: el documento cuenta si alguno va a él.
            return await _unitOfWork.RequisicionesEPP.FindAsync(
                r => r.Detalles.Any(d => d.IdEmpleadoDestino == idEmpleado),
                includeProperties: IncludeListado);
        }

        public async Task<IEnumerable<RequisicionEPP>> GetByEstadoAsync(EstadoRequisicion estado)
        {
            return await _unitOfWork.RequisicionesEPP.FindAsync(
                r => r.EstadoRequisicion == estado,
                includeProperties: IncludeListado);
        }

        public async Task<IEnumerable<RequisicionEPP>> GetPendientesAprobacionAsync()
        {
            return await _unitOfWork.RequisicionesEPP.FindAsync(
                r => r.Detalles.Any(d => d.EstadoRenglon == EstadoRenglonRequisicion.Solicitado),
                includeProperties: IncludeListado);
        }

        public async Task<IEnumerable<RequisicionEPP>> GetPendientesEntregaAsync()
        {
            return await _unitOfWork.RequisicionesEPP.FindAsync(
                r => r.Detalles.Any(d => d.EstadoRenglon == EstadoRenglonRequisicion.Reservado),
                includeProperties: IncludeListado);
        }

        public async Task<ResultadoPaginado<RequisicionEPP>> GetPaginadoAsync(
            ConsultaPaginada consulta, string? filtro = null, string? userId = null)
        {
            var texto = consulta.TextoLimpio;

            return await _unitOfWork.RequisicionesEPP.GetPaginadoAsync(
                consulta,
                filtro: r =>
                    (filtro != "mis" || r.IdUsuarioSolicita == userId) &&
                    (filtro != "pendientes-aprobacion" ||
                     r.Detalles.Any(d => d.EstadoRenglon == EstadoRenglonRequisicion.Solicitado)) &&
                    (filtro != "pendientes-entrega" ||
                     r.Detalles.Any(d => d.EstadoRenglon == EstadoRenglonRequisicion.Reservado ||
                                         d.EstadoRenglon == EstadoRenglonRequisicion.Recibido)) &&
                    (texto == null ||
                     r.NumeroRequisicion.Contains(texto) ||
                     (r.Justificacion != null && r.Justificacion.Contains(texto)) ||
                     (r.Almacen != null && r.Almacen.Nombre.Contains(texto)) ||
                     // Por las columnas del empleado destino: NombreCompleto es
                     // calculada y EF no la traduce a SQL.
                     r.Detalles.Any(d => d.EmpleadoDestino != null &&
                                         (d.EmpleadoDestino.Nombre.Contains(texto) ||
                                          d.EmpleadoDestino.ApellidoPaterno.Contains(texto) ||
                                          d.EmpleadoDestino.NumeroNomina.Contains(texto))) ||
                     r.Detalles.Any(d => d.Material != null && d.Material.Nombre.Contains(texto))),
                orden: q => q.OrderByDescending(r => r.FechaSolicitud).ThenByDescending(r => r.IdRequisicion),
                includeProperties: IncludeListado);
        }

        public async Task<RequisicionEPP?> GetByIdAsync(int id)
        {
            var requisicion = await _unitOfWork.RequisicionesEPP.GetByIdAsync(id, includeProperties: IncludeCompleto);
            if (requisicion == null) return null;

            // Segunda consulta para las firmas. Al compartir el contexto, EF las
            // engancha solo a requisicion.Entregas.
            await _unitOfWork.RequisicionesEntregas.FindAsync(
                e => e.IdRequisicion == id,
                includeProperties: "Empleado,UsuarioEntrega");

            // Y otra para las coberturas, que dicen cuánto de cada renglón ya llegó
            // y está apartado. Van aparte por lo mismo que las firmas: sumar
            // caminos de include a una sola consulta dispara el tiempo que EF tarda
            // en generar el SQL.
            var idsDetalle = requisicion.Detalles.Select(d => d.IdRequisicionDetalle).ToList();
            await _unitOfWork.RequisicionesCobertura.FindAsync(
                c => idsDetalle.Contains(c.IdRequisicionDetalle));

            // Y una última para lo que ampara cada firma. Es lo que se imprime
            // debajo de ella: material, cuánto y de qué lote salió. Deducirlo de
            // los renglones no alcanza, porque un renglón entregado en dos actos
            // guarda un solo acumulado y un solo lote.
            var idsFirma = requisicion.Entregas.Select(e => e.IdRequisicionEntrega).ToList();
            if (idsFirma.Count > 0)
            {
                await _unitOfWork.EntregasEPP.FindAsync(
                    e => e.IdRequisicionEntrega != null && idsFirma.Contains(e.IdRequisicionEntrega.Value),
                    includeProperties: "CompraDetalle.Material.UnidadMedida");
            }

            return requisicion;
        }

        public async Task<RequisicionEPP> CreateAsync(RequisicionEPP requisicion, string userId)
        {
            var almacen = await _unitOfWork.Almacenes.GetByIdAsync(requisicion.IdAlmacen);
            if (almacen == null)
                throw new ArgumentException($"El almacén con ID {requisicion.IdAlmacen} no existe.");

            if (requisicion.Detalles == null || !requisicion.Detalles.Any())
                throw new InvalidOperationException("La requisición debe tener al menos un material.");

            foreach (var detalle in requisicion.Detalles)
            {
                var material = await _unitOfWork.Materiales.GetByIdAsync(detalle.IdMaterial);
                if (material == null)
                    throw new ArgumentException($"El material con ID {detalle.IdMaterial} no existe.");

                if (detalle.CantidadSolicitada <= 0)
                    throw new ArgumentException("La cantidad solicitada debe ser mayor a 0.");

                // Un renglón va a una persona o se carga a la obra, nunca a las dos
                // ni a ninguna. Sin destino, el material saldría del almacén sin
                // que nadie pueda decir adónde fue.
                var vaAPersona = detalle.IdEmpleadoDestino.HasValue;
                var vaAObra = detalle.IdProyectoDestino.HasValue || detalle.IdConceptoPartida.HasValue;

                if (!vaAPersona && !vaAObra)
                {
                    throw new ArgumentException(
                        $"El renglón de '{material.Nombre}' necesita un destino: " +
                        "un trabajador que lo reciba, o la obra o partida a la que se carga.");
                }

                if (vaAPersona && vaAObra)
                {
                    throw new ArgumentException(
                        $"El renglón de '{material.Nombre}' tiene destinatario y obra a la vez. " +
                        "Lo que se entrega a una persona lo firma ella; lo que se carga a la " +
                        "obra no tiene quien firme. Elija uno.");
                }

                if (vaAPersona)
                {
                    var empleado = await _unitOfWork.Empleados.GetByIdAsync(detalle.IdEmpleadoDestino!.Value);
                    if (empleado == null)
                        throw new ArgumentException($"El empleado con ID {detalle.IdEmpleadoDestino} no existe.");
                }
                else
                {
                    // Con partida, la obra se deduce de ella: así no pueden quedar en
                    // desacuerdo, que es de lo que vive un costeo mal cargado.
                    if (detalle.IdConceptoPartida.HasValue)
                    {
                        var partida = await _unitOfWork.ConceptosPartidas
                            .GetByIdAsync(detalle.IdConceptoPartida.Value);

                        if (partida == null)
                            throw new ArgumentException($"La partida con ID {detalle.IdConceptoPartida} no existe.");

                        detalle.IdProyectoDestino = partida.IdProyecto;
                    }
                    else
                    {
                        var proyecto = await _unitOfWork.Proyectos
                            .GetByIdAsync(detalle.IdProyectoDestino!.Value);

                        if (proyecto == null)
                            throw new ArgumentException($"La obra con ID {detalle.IdProyectoDestino} no existe.");
                    }
                }

                detalle.EstadoRenglon = EstadoRenglonRequisicion.Solicitado;
            }

            requisicion.NumeroRequisicion = await GenerarNumeroRequisicionAsync();
            requisicion.IdUsuarioSolicita = userId;
            requisicion.FechaSolicitud = DateTime.Now;
            requisicion.EstadoRequisicion = EstadoRequisicion.Pendiente;

            await _unitOfWork.RequisicionesEPP.AddAsync(requisicion);
            await _unitOfWork.CompleteAsync();

            return requisicion;
        }

        public async Task<bool> AprobarAsync(int id, string userId, bool aprobada, string? motivoRechazo,
            List<int>? idsRenglones = null)
        {
            var requisicion = await _unitOfWork.RequisicionesEPP.GetByIdAsync(id, includeProperties: "Detalles");
            if (requisicion == null)
                return false;

            // Segregación de funciones: quien solicita no puede autorizar lo suyo.
            if (requisicion.IdUsuarioSolicita == userId)
                throw new InvalidOperationException(
                    "No puede aprobar ni rechazar una requisición que usted mismo solicitó. " +
                    "Debe autorizarla otra persona.");

            if (!aprobada && string.IsNullOrWhiteSpace(motivoRechazo))
                throw new ArgumentException("Debe especificar el motivo del rechazo.");

            // Sin lista explícita, se resuelve todo lo que siga solicitado.
            var objetivo = requisicion.Detalles
                .Where(d => d.EstadoRenglon == EstadoRenglonRequisicion.Solicitado)
                .Where(d => idsRenglones == null || idsRenglones.Count == 0
                            || idsRenglones.Contains(d.IdRequisicionDetalle))
                .ToList();

            if (objetivo.Count == 0)
                throw new InvalidOperationException("No hay renglones pendientes de autorizar en esta requisición.");

            if (!aprobada)
            {
                foreach (var detalle in objetivo)
                {
                    detalle.EstadoRenglon = EstadoRenglonRequisicion.Rechazado;
                    detalle.MotivoRechazo = motivoRechazo;
                }
            }
            else
            {
                // Autorizar resuelve enseguida cada renglón contra la existencia de
                // su almacén. El reparto va del más antiguo al más nuevo: cuando el
                // stock no alcanza para todos, se lo lleva quien pidió primero, que
                // es un criterio justo y explicable al que se queda esperando.
                foreach (var detalle in objetivo.OrderBy(d => d.IdRequisicionDetalle))
                {
                    detalle.EstadoRenglon = EstadoRenglonRequisicion.Autorizado;
                    detalle.MotivoRechazo = null;

                    var reservado = await _inventarioService.ReservarAsync(
                        detalle.IdMaterial, requisicion.IdAlmacen, detalle.CantidadSolicitada,
                        userId, requisicion.IdRequisicion, requisicion.NumeroRequisicion);

                    detalle.EstadoRenglon = reservado
                        ? EstadoRenglonRequisicion.Reservado
                        : EstadoRenglonRequisicion.PorComprar;
                }
            }

            requisicion.IdUsuarioAprueba = userId;
            requisicion.FechaAprobacion = DateTime.Now;
            requisicion.MotivoRechazo = aprobada ? null : motivoRechazo;
            requisicion.EstadoRequisicion = requisicion.CalcularEstado();

            _unitOfWork.RequisicionesEPP.Update(requisicion);

            try
            {
                return await _unitOfWork.CompleteAsync() > 0;
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException(
                    "Otra persona movió la existencia de alguno de estos materiales " +
                    "mientras autorizaba. Vuelva a intentarlo para repartir sobre el " +
                    "stock actual.");
            }
        }

        /// <summary>
        /// Cuánto material hay apartado a nombre de un renglón.
        ///
        /// Cuando se reservó de la existencia que ya había, es todo lo solicitado.
        /// Cuando vino de una orden de compra, es lo que de verdad ha llegado, que
        /// puede ser una parte si el proveedor entregó a medias.
        /// </summary>
        private async Task<decimal> ApartadoDelRenglonAsync(RequisicionEPPDetalle detalle)
        {
            var coberturas = await _unitOfWork.RequisicionesCobertura.FindAsync(
                c => c.IdRequisicionDetalle == detalle.IdRequisicionDetalle);

            var lista = coberturas.ToList();

            return lista.Count == 0
                ? detalle.CantidadSolicitada
                : lista.Sum(c => c.CantidadApartada);
        }

        /// <summary>
        /// De qué lotes sale lo que se va a entregar de un renglón.
        ///
        /// Una recepción parcial parte el material en varios lotes —llegaron 5 y
        /// después 3—, así que pedir que el almacenista elija uno solo dejaba
        /// renglones imposibles de surtir aunque hubiera existencia de sobra. Aquí
        /// se reparte automáticamente: sale primero lo que caduca antes y, a
        /// igualdad, lo que lleva más tiempo en la bodega.
        ///
        /// Con una talla pedida sólo entran los lotes de esa talla: dar el número
        /// equivocado no es surtir, es devolver el problema a la obra.
        /// </summary>
        private async Task<(List<(CompraEPPDetalle Lote, decimal Cantidad)> Reparto, string? Error)>
            RepartirEnLotesAsync(RequisicionEPPDetalle detalle, int idAlmacen, decimal cantidad)
        {
            var reparto = new List<(CompraEPPDetalle, decimal)>();

            var lotes = (await _compraService.GetLotesDisponiblesAsync(detalle.IdMaterial, idAlmacen)).ToList();

            // Sólo se filtra por talla donde la talla significa algo. Un consumible
            // puede traer una talla capturada por descuido, y exigir que el lote la
            // repita dejaba sin surtir material que estaba ahí, en el anaquel.
            var importaLaTalla = detalle.Material?.RequiereTalla == true
                                 && !string.IsNullOrWhiteSpace(detalle.TallaSolicitada);

            if (importaLaTalla)
            {
                lotes = lotes
                    .Where(l => string.Equals(l.Talla, detalle.TallaSolicitada, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var disponible = lotes.Sum(l => l.CantidadDisponible);
            if (disponible < cantidad)
            {
                var conTalla = importaLaTalla ? $" en talla {detalle.TallaSolicitada}" : "";

                return (reparto,
                    $"No hay existencia suficiente de '{detalle.Material?.Nombre ?? $"material {detalle.IdMaterial}"}'{conTalla}. " +
                    $"Disponible: {disponible}, se quiere entregar: {cantidad}.");
            }

            var porRepartir = cantidad;
            foreach (var lote in lotes)
            {
                if (porRepartir <= 0) break;

                var toma = Math.Min(porRepartir, lote.CantidadDisponible);
                if (toma <= 0) continue;

                reparto.Add((lote, toma));
                porRepartir -= toma;
            }

            return (reparto, null);
        }

        public async Task<(bool Success, string? Error)> EntregarAEmpleadoAsync(
            int id,
            int idEmpleado,
            string userId,
            string firmaDigital,
            string? fotoEvidencia,
            string? observaciones,
            List<(int IdDetalle, int? IdCompraDetalle, decimal CantidadEntregada)> detalles)
        {
            var requisicion = await _unitOfWork.RequisicionesEPP.GetByIdAsync(
                id, includeProperties: "Detalles.Material,Entregas");

            if (requisicion == null)
                return (false, "Requisición no encontrada.");

            if (string.IsNullOrWhiteSpace(firmaDigital))
                return (false, "La firma digital es obligatoria.");

            if (detalles == null || detalles.Count == 0)
                return (false, "Debe especificar al menos un material a entregar.");

            var empleado = await _unitOfWork.Empleados.GetByIdAsync(idEmpleado);
            if (empleado == null)
                return (false, $"El empleado con ID {idEmpleado} no existe.");

            // Antes se exigía una sola firma por persona por documento. Con material
            // que llega en partes eso deja la segunda mitad sin poder entregarse:
            // la persona ya firmó, y lo que le falta se queda en la bodega. Ahora
            // firma cada vez que se lleva algo, y lo que impide entregar dos veces
            // el mismo renglón es el estado del renglón, no la firma.
            // Un renglón puede venir más de una vez cuando el almacenista elige
            // lotes a mano; lo que no puede es sumar más de lo que se pidió.
            var porRenglon = detalles
                .GroupBy(d => d.IdDetalle)
                .ToDictionary(g => g.Key, g => g.Sum(d => d.CantidadEntregada));

            // Validación completa antes de mover nada.
            foreach (var entrega in detalles)
            {
                var detalle = requisicion.Detalles.FirstOrDefault(d => d.IdRequisicionDetalle == entrega.IdDetalle);
                if (detalle == null)
                    return (false, $"Detalle {entrega.IdDetalle} no encontrado.");

                // Cada firma sólo puede amparar lo que recibe esa persona.
                if (detalle.IdEmpleadoDestino != idEmpleado)
                    return (false,
                        $"El renglón {entrega.IdDetalle} no corresponde a {empleado.NombreCompleto}. " +
                        "Cada empleado firma únicamente lo que recibe.");

                // Sólo se surte lo que tiene material apartado: lo que estaba en el
                // almacén al autorizar, o lo que llegó después contra la orden de
                // compra. Un renglón por comprar sigue esperando su pedido, y uno
                // pedido espera a que el proveedor entregue.
                if (detalle.EstadoRenglon != EstadoRenglonRequisicion.Reservado &&
                    detalle.EstadoRenglon != EstadoRenglonRequisicion.Recibido &&
                    detalle.EstadoRenglon != EstadoRenglonRequisicion.Autorizado)
                {
                    var motivo = detalle.EstadoRenglon switch
                    {
                        EstadoRenglonRequisicion.PorComprar =>
                            "está pendiente de compra: no hay existencia apartada para surtirlo",
                        EstadoRenglonRequisicion.EnOrdenCompra =>
                            "está pedido al proveedor y todavía no se recibe",
                        _ => $"no está listo para entregarse (estado actual: {detalle.EstadoRenglon})"
                    };

                    return (false, $"El renglón {entrega.IdDetalle} {motivo}.");
                }

                if (entrega.CantidadEntregada <= 0)
                    return (false, $"La cantidad entregada del renglón {entrega.IdDetalle} debe ser mayor a 0.");

                // Lo entregado antes cuenta: un renglón se puede surtir en varias
                // vueltas cuando el material llega en partes.
                var yaEntregado = detalle.CantidadEntregada ?? 0;
                var totalAhora = porRenglon[entrega.IdDetalle];

                if (yaEntregado + totalAhora > detalle.CantidadSolicitada)
                    return (false,
                        $"No se puede entregar más de lo solicitado. " +
                        $"Solicitado: {detalle.CantidadSolicitada}, ya entregado: {yaEntregado}, " +
                        $"a entregar ahora: {totalAhora}.");

                // Un renglón con material apartado sólo puede llevarse lo suyo. El
                // resto de la existencia puede estar apartada para otra persona, y
                // la pantalla de entrega sólo ve los lotes, que no saben de eso.
                if (detalle.TieneReserva)
                {
                    var apartado = await ApartadoDelRenglonAsync(detalle);
                    var suyo = apartado - yaEntregado;

                    if (totalAhora > suyo)
                        return (false,
                            $"Sólo hay {suyo:0.##} apartado a nombre de esta persona para " +
                            $"'{detalle.Material?.Nombre ?? $"material {detalle.IdMaterial}"}'. " +
                            (apartado < detalle.CantidadSolicitada
                                ? "El resto todavía no llega del proveedor."
                                : "Ya se entregó el resto."));
                }

                // Sin lote indicado, el sistema reparte solo. Con lote indicado
                // manda el almacenista: habrá visto algo en el anaquel que el
                // sistema no sabe.
                if (entrega.IdCompraDetalle.HasValue)
                {
                    var lote = await _unitOfWork.ComprasEPPDetalle.GetByIdAsync(entrega.IdCompraDetalle.Value);
                    if (lote == null)
                        return (false, $"El lote {entrega.IdCompraDetalle} no existe.");

                    if (lote.IdMaterial != detalle.IdMaterial)
                        return (false, $"El lote {entrega.IdCompraDetalle} no corresponde al material solicitado.");

                    if (lote.IdAlmacen != requisicion.IdAlmacen)
                        return (false,
                            $"El lote {entrega.IdCompraDetalle} pertenece a otro almacén y no puede surtir " +
                            "esta requisición.");

                    if (lote.CantidadDisponible < entrega.CantidadEntregada)
                        return (false, $"El lote {entrega.IdCompraDetalle} no tiene suficiente cantidad. Disponible: {lote.CantidadDisponible}");
                }
                else
                {
                    var (_, errorReparto) = await RepartirEnLotesAsync(
                        detalle, requisicion.IdAlmacen, entrega.CantidadEntregada);

                    if (errorReparto != null) return (false, errorReparto);
                }
            }

            var transaccionPropia = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // La firma de este acto de entrega, que ampara sólo lo que esta
                // persona se llevó hoy. Se graba antes que las salidas porque cada
                // una nace apuntando a ella: sin ese sello la ficha no puede saber
                // qué cubrió cada firma cuando la misma persona firma dos veces.
                var firma = new RequisicionEntrega
                {
                    IdRequisicion = requisicion.IdRequisicion,
                    IdEmpleado = idEmpleado,
                    FechaEntrega = DateTime.Now,
                    IdUsuarioEntrega = userId,
                    FirmaDigital = firmaDigital,
                    FotoEvidencia = fotoEvidencia,
                    Observaciones = observaciones
                };
                await _unitOfWork.RequisicionesEntregas.AddAsync(firma);
                await _unitOfWork.CompleteAsync();

                // La salida de almacén pasa por EntregaEPPService, que descuenta lote
                // e inventario, evalúa las alertas de consumo y recalcula estadísticas.
                foreach (var entrega in detalles)
                {
                    var detalle = requisicion.Detalles.First(d => d.IdRequisicionDetalle == entrega.IdDetalle);

                    // De dónde sale: del lote que se eligió, o del reparto automático.
                    List<(CompraEPPDetalle Lote, decimal Cantidad)> reparto;

                    if (entrega.IdCompraDetalle.HasValue)
                    {
                        var elegido = await _unitOfWork.ComprasEPPDetalle.GetByIdAsync(entrega.IdCompraDetalle.Value);
                        reparto = new List<(CompraEPPDetalle, decimal)> { (elegido!, entrega.CantidadEntregada) };
                    }
                    else
                    {
                        (reparto, _) = await RepartirEnLotesAsync(
                            detalle, requisicion.IdAlmacen, entrega.CantidadEntregada);
                    }

                    // La reserva se consume por lo que de verdad sale del anaquel y
                    // no por lo solicitado: si se entrega a medias, lo que falta
                    // sigue apartado a nombre de esta persona.
                    if (detalle.TieneReserva)
                    {
                        await _inventarioService.ConsumirReservaAsync(
                            detalle.IdMaterial, requisicion.IdAlmacen, entrega.CantidadEntregada,
                            userId, requisicion.IdRequisicion, requisicion.NumeroRequisicion);
                    }

                    // Una salida por lote: cada una conserva el costo y la factura
                    // de la que salió ese material.
                    foreach (var (lote, cantidad) in reparto)
                    {
                        await _entregaService.CreateEntregaAsync(new EntregaEPP
                        {
                            IdEmpleado = idEmpleado,
                            IdCompraDetalle = lote.IdCompraDetalle,
                            IdRequisicionEntrega = firma.IdRequisicionEntrega,
                            // Y a qué renglón: con varios del mismo material en un
                            // documento, la firma sola no lo distingue.
                            IdRequisicionDetalle = detalle.IdRequisicionDetalle,
                            FechaEntrega = DateTime.Now,
                            CantidadEntregada = cantidad,
                            TallaEntregada = detalle.TallaSolicitada ?? lote.Talla ?? string.Empty,
                            Observaciones = $"Requisición: {requisicion.NumeroRequisicion}"
                        }, userId);

                        detalle.IdCompraDetalle ??= lote.IdCompraDetalle;
                    }

                    detalle.CantidadEntregada = (detalle.CantidadEntregada ?? 0) + entrega.CantidadEntregada;

                    // El renglón se cierra sólo cuando se entregó todo lo pedido.
                    // Antes se marcaba surtido pasara lo que pasara, así que
                    // entregar de menos lo cerraba y el faltante se perdía sin que
                    // nadie se enterara.
                    if (detalle.CantidadEntregada >= detalle.CantidadSolicitada)
                        detalle.EstadoRenglon = EstadoRenglonRequisicion.Surtido;
                }

                // Cada renglón queda amarrado a la firma que lo respalda: con varias
                // firmas por documento, es lo único que dice cuál cubrió qué.
                foreach (var entrega in detalles)
                {
                    var detalle = requisicion.Detalles.First(d => d.IdRequisicionDetalle == entrega.IdDetalle);
                    detalle.IdRequisicionEntrega = firma.IdRequisicionEntrega;
                    _unitOfWork.RequisicionesEPPDetalle.Update(detalle);
                }

                // El documento queda parcial mientras otras personas no hayan recibido.
                requisicion.EstadoRequisicion = requisicion.CalcularEstado();
                _unitOfWork.RequisicionesEPP.Update(requisicion);

                await _unitOfWork.CompleteAsync();
                if (transaccionPropia) await _unitOfWork.CommitTransactionAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                if (transaccionPropia) await _unitOfWork.RollbackTransactionAsync();
                return (false, ex.Message);
            }
        }

        public async Task<bool> CancelarAsync(int id, string userId)
        {
            var requisicion = await _unitOfWork.RequisicionesEPP.GetByIdAsync(id, includeProperties: "Detalles");
            if (requisicion == null)
                return false;

            if (requisicion.IdUsuarioSolicita != userId)
                throw new InvalidOperationException("Solo el solicitante puede cancelar la requisición.");

            if (requisicion.Detalles.Any(d => d.EstaSurtido))
                throw new InvalidOperationException(
                    "No se puede cancelar: ya se entregó material de esta requisición. " +
                    "Los renglones pendientes pueden cancelarse uno por uno.");

            var vivos = requisicion.Detalles.Where(d => d.EstaPendiente).ToList();
            if (vivos.Count == 0)
                throw new InvalidOperationException("La requisición no tiene renglones que cancelar.");

            // Lo apartado por orden de compra se sabe en la cobertura; lo apartado
            // de la existencia que ya había es la cantidad solicitada completa.
            var idsVivos = vivos.Select(d => d.IdRequisicionDetalle).ToList();
            var coberturas = (await _unitOfWork.RequisicionesCobertura.FindAsync(
                    c => idsVivos.Contains(c.IdRequisicionDetalle)))
                .GroupBy(c => c.IdRequisicionDetalle)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.CantidadApartada));

            foreach (var detalle in vivos)
            {
                // Lo apartado vuelve a estar disponible para quien venga detrás,
                // pero sólo lo que de verdad sigue apartado: ni lo que nunca llegó
                // ni lo que esta persona ya se llevó.
                if (detalle.TieneReserva)
                {
                    var apartado = coberturas.TryGetValue(detalle.IdRequisicionDetalle, out var porOrden)
                        ? porOrden
                        : detalle.CantidadSolicitada;

                    var porLiberar = apartado - (detalle.CantidadEntregada ?? 0);

                    if (porLiberar > 0)
                    {
                        await _inventarioService.LiberarReservaAsync(
                            detalle.IdMaterial, requisicion.IdAlmacen, porLiberar,
                            userId, requisicion.IdRequisicion, requisicion.NumeroRequisicion,
                            motivo: "se canceló la requisición.");
                    }
                }

                detalle.EstadoRenglon = EstadoRenglonRequisicion.Cancelado;
            }

            requisicion.EstadoRequisicion = requisicion.CalcularEstado();

            _unitOfWork.RequisicionesEPP.Update(requisicion);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<string> GenerarNumeroRequisicionAsync()
        {
            var año = DateTime.Now.Year;
            var prefijo = $"REQ-{año}-";

            var requisiciones = await _unitOfWork.RequisicionesEPP.FindAsync(
                r => r.NumeroRequisicion.StartsWith(prefijo));

            var ultimoNumero = 0;
            if (requisiciones.Any())
            {
                ultimoNumero = requisiciones
                    .Select(r => int.TryParse(r.NumeroRequisicion.Replace(prefijo, ""), out var num) ? num : 0)
                    .Max();
            }

            return $"{prefijo}{(ultimoNumero + 1):D4}";
        }

        public async Task<bool> PuedeVerRequisicionAsync(int idRequisicion, string userId)
        {
            var requisicion = await _unitOfWork.RequisicionesEPP.GetByIdAsync(idRequisicion);
            return requisicion != null && requisicion.IdUsuarioSolicita == userId;
        }
    }
}
