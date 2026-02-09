using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    public class RequisicionEPPService : IRequisicionEPPService
    {
        private readonly IUnitOfWork _unitOfWork;

        //private const string IncludeProperties = "UsuarioSolicita,EmpleadoRecibe.Proyecto,Almacen.Proyecto,UsuarioAprueba,UsuarioEntrega,Detalles.Material.UnidadMedida,Detalles.Compra.Proveedor";
        private const string IncludeProperties = "UsuarioSolicita,EmpleadoRecibe,Almacen,UsuarioAprueba,UsuarioEntrega,Detalles.Material.UnidadMedida,Detalles.Compra";
        public RequisicionEPPService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<RequisicionEPP>> GetAllAsync()
        {
            return await _unitOfWork.RequisicionesEPP.GetAllAsync();
        }

        public async Task<IEnumerable<RequisicionEPP>> GetByUsuarioAsync(string userId)
        {
            return await _unitOfWork.RequisicionesEPP.FindAsync(
                r => r.IdUsuarioSolicita == userId,
                includeProperties: IncludeProperties);
        }

        public async Task<IEnumerable<RequisicionEPP>> GetByEmpleadoAsync(int idEmpleado)
        {
            return await _unitOfWork.RequisicionesEPP.FindAsync(
                r => r.IdEmpleadoRecibe == idEmpleado,
                includeProperties: IncludeProperties);
        }

        public async Task<IEnumerable<RequisicionEPP>> GetByEstadoAsync(EstadoRequisicion estado)
        {
            return await _unitOfWork.RequisicionesEPP.FindAsync(
                r => r.EstadoRequisicion == estado,
                includeProperties: IncludeProperties);
        }

        public async Task<IEnumerable<RequisicionEPP>> GetPendientesAprobacionAsync()
        {
            return await GetByEstadoAsync(EstadoRequisicion.Pendiente);
        }

        public async Task<IEnumerable<RequisicionEPP>> GetPendientesEntregaAsync()
        {
            return await GetByEstadoAsync(EstadoRequisicion.Aprobada);
        }

        public async Task<RequisicionEPP?> GetByIdAsync(int id)
        {
            return await _unitOfWork.RequisicionesEPP.GetByIdAsync(id, includeProperties: IncludeProperties);
        }

        public async Task<RequisicionEPP> CreateAsync(RequisicionEPP requisicion, string userId)
        {
            // Validar empleado
            var empleado = await _unitOfWork.Empleados.GetByIdAsync(requisicion.IdEmpleadoRecibe);
            if (empleado == null)
                throw new ArgumentException($"El empleado con ID {requisicion.IdEmpleadoRecibe} no existe.");

            // Validar almacén
            var almacen = await _unitOfWork.Almacenes.GetByIdAsync(requisicion.IdAlmacen);
            if (almacen == null)
                throw new ArgumentException($"El almacén con ID {requisicion.IdAlmacen} no existe.");

            // Validar que tenga detalles
            if (requisicion.Detalles == null || !requisicion.Detalles.Any())
                throw new InvalidOperationException("La requisición debe tener al menos un material.");

            // Validar materiales
            foreach (var detalle in requisicion.Detalles)
            {
                var material = await _unitOfWork.MaterialesEPP.GetByIdAsync(detalle.IdMaterial);
                if (material == null)
                    throw new ArgumentException($"El material con ID {detalle.IdMaterial} no existe.");

                if (detalle.CantidadSolicitada <= 0)
                    throw new ArgumentException("La cantidad solicitada debe ser mayor a 0.");
            }

            // Asignar valores
            requisicion.NumeroRequisicion = await GenerarNumeroRequisicionAsync();
            requisicion.IdUsuarioSolicita = userId;
            requisicion.EstadoRequisicion = EstadoRequisicion.Pendiente;
            requisicion.FechaSolicitud = DateTime.Now;

            await _unitOfWork.RequisicionesEPP.AddAsync(requisicion);
            await _unitOfWork.CompleteAsync();

            return requisicion;
        }

        public async Task<bool> AprobarAsync(int id, string userId, bool aprobada, string? motivoRechazo)
        {
            var requisicion = await _unitOfWork.RequisicionesEPP.GetByIdAsync(id);
            if (requisicion == null)
                return false;

            if (requisicion.EstadoRequisicion != EstadoRequisicion.Pendiente)
                throw new InvalidOperationException("Solo se pueden aprobar/rechazar requisiciones pendientes.");

            if (!aprobada && string.IsNullOrWhiteSpace(motivoRechazo))
                throw new ArgumentException("Debe especificar el motivo del rechazo.");

            requisicion.EstadoRequisicion = aprobada ? EstadoRequisicion.Aprobada : EstadoRequisicion.Rechazada;
            requisicion.IdUsuarioAprueba = userId;
            requisicion.FechaAprobacion = DateTime.Now;
            requisicion.MotivoRechazo = aprobada ? null : motivoRechazo;

            _unitOfWork.RequisicionesEPP.Update(requisicion);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<(bool Success, string? Error)> EntregarAsync(
            int id,
            string userId,
            string firmaDigital,
            string? fotoEvidencia,
            string? observaciones,
            List<(int IdDetalle, int IdCompra, decimal CantidadEntregada)> detalles)
        {
            var requisicion = await _unitOfWork.RequisicionesEPP.GetByIdAsync(id, includeProperties: "Detalles");
            if (requisicion == null)
                return (false, "Requisición no encontrada.");

            if (requisicion.EstadoRequisicion != EstadoRequisicion.Aprobada)
                return (false, "Solo se pueden entregar requisiciones aprobadas.");

            if (string.IsNullOrWhiteSpace(firmaDigital))
                return (false, "La firma digital es obligatoria.");

            // Procesar cada detalle
            foreach (var entrega in detalles)
            {
                var detalle = requisicion.Detalles.FirstOrDefault(d => d.IdRequisicionDetalle == entrega.IdDetalle);
                if (detalle == null)
                    return (false, $"Detalle {entrega.IdDetalle} no encontrado.");

                // Validar lote
                var compra = await _unitOfWork.ComprasEPP.GetByIdAsync(entrega.IdCompra);
                if (compra == null)
                    return (false, $"El lote {entrega.IdCompra} no existe.");

                if (compra.IdMaterial != detalle.IdMaterial)
                    return (false, $"El lote {entrega.IdCompra} no corresponde al material solicitado.");

                if (compra.CantidadDisponible < entrega.CantidadEntregada)
                    return (false, $"El lote {entrega.IdCompra} no tiene suficiente cantidad. Disponible: {compra.CantidadDisponible}");

                // Descontar del lote
                compra.CantidadDisponible -= entrega.CantidadEntregada;
                _unitOfWork.ComprasEPP.Update(compra);

                // Actualizar inventario
                var inventarios = await _unitOfWork.Inventarios.FindAsync(
                    i => i.IdMaterial == detalle.IdMaterial && i.IdAlmacen == requisicion.IdAlmacen);
                var inventario = inventarios.FirstOrDefault();

                if (inventario != null)
                {
                    inventario.Existencia -= entrega.CantidadEntregada;
                    inventario.FechaUltimoMovimiento = DateTime.Now;
                    _unitOfWork.Inventarios.Update(inventario);
                }

                // Actualizar detalle
                detalle.IdCompra = entrega.IdCompra;
                detalle.CantidadEntregada = entrega.CantidadEntregada;

                // Crear registro de EntregaEPP
                var entregaEPP = new EntregaEPP
                {
                    IdEmpleado = requisicion.IdEmpleadoRecibe,
                    IdCompra = entrega.IdCompra,
                    FechaEntrega = DateTime.Now,
                    CantidadEntregada = entrega.CantidadEntregada,
                    TallaEntregada = detalle.TallaSolicitada ?? string.Empty,
                    Observaciones = $"Requisición: {requisicion.NumeroRequisicion}"
                };

                await _unitOfWork.EntregasEPP.AddAsync(entregaEPP);
            }

            // Actualizar requisición
            requisicion.EstadoRequisicion = EstadoRequisicion.Entregada;
            requisicion.IdUsuarioEntrega = userId;
            requisicion.FechaEntrega = DateTime.Now;
            requisicion.FirmaDigital = firmaDigital;
            requisicion.FotoEvidencia = fotoEvidencia;
            requisicion.Observaciones = observaciones;

            _unitOfWork.RequisicionesEPP.Update(requisicion);
            await _unitOfWork.CompleteAsync();

            return (true, null);
        }

        public async Task<bool> CancelarAsync(int id, string userId)
        {
            var requisicion = await _unitOfWork.RequisicionesEPP.GetByIdAsync(id);
            if (requisicion == null)
                return false;

            if (requisicion.EstadoRequisicion != EstadoRequisicion.Pendiente)
                throw new InvalidOperationException("Solo se pueden cancelar requisiciones pendientes.");

            if (requisicion.IdUsuarioSolicita != userId)
                throw new InvalidOperationException("Solo el solicitante puede cancelar la requisición.");

            requisicion.EstadoRequisicion = EstadoRequisicion.Cancelada;
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
            var requisicion = await _unitOfWork.RequisicionesEPP.GetByIdAsync(idRequisicion, includeProperties: "EmpleadoRecibe");
            if (requisicion == null)
                return false;

            // El solicitante puede ver
            if (requisicion.IdUsuarioSolicita == userId)
                return true;

            // Buscar si el usuario está asociado al empleado receptor
            var empleadosUsuario = await _unitOfWork.Empleados.FindAsync(e => e.IdEmpleado == requisicion.IdEmpleadoRecibe);

            return empleadosUsuario.Any();
        }
    }
}