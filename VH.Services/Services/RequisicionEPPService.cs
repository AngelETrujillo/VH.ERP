using Microsoft.EntityFrameworkCore;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    public class RequisicionEPPService : IRequisicionEPPService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEntregaEPPService _entregaService;
        private readonly IInventarioService _inventarioService;

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
            IInventarioService inventarioService)
        {
            _unitOfWork = unitOfWork;
            _entregaService = entregaService;
            _inventarioService = inventarioService;
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

        public async Task<RequisicionEPP?> GetByIdAsync(int id)
        {
            var requisicion = await _unitOfWork.RequisicionesEPP.GetByIdAsync(id, includeProperties: IncludeCompleto);
            if (requisicion == null) return null;

            // Segunda consulta para las firmas. Al compartir el contexto, EF las
            // engancha solo a requisicion.Entregas.
            await _unitOfWork.RequisicionesEntregas.FindAsync(
                e => e.IdRequisicion == id,
                includeProperties: "Empleado,UsuarioEntrega");

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

                // El modelo ya admite cargar un renglón a la obra o a una partida,
                // pero surtirlo exige una salida de almacén que no vaya a una
                // persona, y eso llega con el kardex de movimientos. Aceptarlo hoy
                // dejaría renglones autorizados que nadie puede despachar.
                if (!detalle.IdEmpleadoDestino.HasValue)
                {
                    throw new ArgumentException(
                        $"El renglón de '{material.Nombre}' necesita un trabajador que lo reciba. " +
                        "El consumo cargado directamente a la obra estará disponible cuando " +
                        "el almacén registre salidas sin destinatario.");
                }

                var empleado = await _unitOfWork.Empleados.GetByIdAsync(detalle.IdEmpleadoDestino.Value);
                if (empleado == null)
                    throw new ArgumentException($"El empleado con ID {detalle.IdEmpleadoDestino} no existe.");

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

        public async Task<(bool Success, string? Error)> EntregarAEmpleadoAsync(
            int id,
            int idEmpleado,
            string userId,
            string firmaDigital,
            string? fotoEvidencia,
            string? observaciones,
            List<(int IdDetalle, int IdCompraDetalle, decimal CantidadEntregada)> detalles)
        {
            var requisicion = await _unitOfWork.RequisicionesEPP.GetByIdAsync(
                id, includeProperties: "Detalles,Entregas");

            if (requisicion == null)
                return (false, "Requisición no encontrada.");

            if (string.IsNullOrWhiteSpace(firmaDigital))
                return (false, "La firma digital es obligatoria.");

            if (detalles == null || detalles.Count == 0)
                return (false, "Debe especificar al menos un material a entregar.");

            var empleado = await _unitOfWork.Empleados.GetByIdAsync(idEmpleado);
            if (empleado == null)
                return (false, $"El empleado con ID {idEmpleado} no existe.");

            // Una persona firma una sola vez por documento.
            if (requisicion.Entregas.Any(e => e.IdEmpleado == idEmpleado))
                return (false, $"{empleado.NombreCompleto} ya firmó la entrega de esta requisición.");

            var repetido = detalles
                .GroupBy(d => d.IdDetalle)
                .FirstOrDefault(g => g.Count() > 1);
            if (repetido != null)
                return (false, $"El renglón {repetido.Key} viene repetido en la entrega.");

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

                // Sólo se surte lo que tiene material apartado. Un renglón por
                // comprar espera a que llegue la orden de compra.
                if (detalle.EstadoRenglon != EstadoRenglonRequisicion.Reservado &&
                    detalle.EstadoRenglon != EstadoRenglonRequisicion.Autorizado)
                {
                    var motivo = detalle.EstadoRenglon == EstadoRenglonRequisicion.PorComprar
                        ? "está pendiente de compra: no hay existencia apartada para surtirlo"
                        : $"no está listo para entregarse (estado actual: {detalle.EstadoRenglon})";

                    return (false, $"El renglón {entrega.IdDetalle} {motivo}.");
                }

                if (entrega.CantidadEntregada <= 0)
                    return (false, $"La cantidad entregada del renglón {entrega.IdDetalle} debe ser mayor a 0.");

                if (entrega.CantidadEntregada > detalle.CantidadSolicitada)
                    return (false,
                        $"No se puede entregar más de lo solicitado. " +
                        $"Solicitado: {detalle.CantidadSolicitada}, a entregar: {entrega.CantidadEntregada}.");

                var lote = await _unitOfWork.ComprasEPPDetalle.GetByIdAsync(entrega.IdCompraDetalle);
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

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // La salida de almacén pasa por EntregaEPPService, que descuenta lote
                // e inventario, evalúa las alertas de consumo y recalcula estadísticas.
                foreach (var entrega in detalles)
                {
                    var detalle = requisicion.Detalles.First(d => d.IdRequisicionDetalle == entrega.IdDetalle);

                    var entregaEPP = new EntregaEPP
                    {
                        IdEmpleado = idEmpleado,
                        IdCompraDetalle = entrega.IdCompraDetalle,
                        FechaEntrega = DateTime.Now,
                        CantidadEntregada = entrega.CantidadEntregada,
                        TallaEntregada = detalle.TallaSolicitada ?? string.Empty,
                        Observaciones = $"Requisición: {requisicion.NumeroRequisicion}"
                    };

                    // La reserva se consume: el material sale del anaquel, así que
                    // deja de estar apartado y de contar en el comprometido.
                    if (detalle.TieneReserva)
                    {
                        await _inventarioService.ConsumirReservaAsync(
                            detalle.IdMaterial, requisicion.IdAlmacen, detalle.CantidadSolicitada,
                            userId, requisicion.IdRequisicion, requisicion.NumeroRequisicion);
                    }

                    await _entregaService.CreateEntregaAsync(entregaEPP, userId);

                    detalle.IdCompraDetalle = entrega.IdCompraDetalle;
                    detalle.CantidadEntregada = entrega.CantidadEntregada;
                    detalle.EstadoRenglon = EstadoRenglonRequisicion.Surtido;
                }

                // La firma de esta persona, que ampara sólo lo que ella recibió.
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

                // El documento queda parcial mientras otras personas no hayan recibido.
                requisicion.EstadoRequisicion = requisicion.CalcularEstado();
                _unitOfWork.RequisicionesEPP.Update(requisicion);

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
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

            foreach (var detalle in vivos)
            {
                // Lo apartado vuelve a estar disponible para quien venga detrás.
                if (detalle.TieneReserva)
                {
                    await _inventarioService.LiberarReservaAsync(
                        detalle.IdMaterial, requisicion.IdAlmacen, detalle.CantidadSolicitada,
                        userId, requisicion.IdRequisicion, requisicion.NumeroRequisicion);
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
