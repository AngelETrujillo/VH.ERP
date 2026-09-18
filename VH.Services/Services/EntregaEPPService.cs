using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    public class EntregaEPPService : IEntregaEPPService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAlertaConsumoService _alertaConsumoService;
        private readonly IDashboardAnalyticsService _dashboardService;
        private readonly IMovimientoInventarioService _movimientoService;

        public EntregaEPPService(IUnitOfWork unitOfWork, IAlertaConsumoService alertaConsumoService,
            IDashboardAnalyticsService dashboardService, IMovimientoInventarioService movimientoService)
        {
            _unitOfWork = unitOfWork;
            _alertaConsumoService = alertaConsumoService;
            _dashboardService = dashboardService;
            _movimientoService = movimientoService;
        }

        private const string IncludeProperties = "Empleado.Proyecto,CompraDetalle.Material.UnidadMedida,CompraDetalle.Almacen,CompraDetalle.Compra.Proveedor";

        /// <summary>
        /// Obtiene el registro de inventario del par material/almacén, o falla.
        ///
        /// Todo movimiento de existencia necesita este registro: si no está, el lote
        /// se movería y la existencia no, y la diferencia no vuelve a aparecer en
        /// ningún lado. Se crea automáticamente en la primera compra, así que su
        /// ausencia significa que alguien lo borró y hay que revisarlo a mano.
        /// </summary>
        private async Task<Inventario> GetInventarioObligatorioAsync(
            int idMaterial, int idAlmacen, string? nombreMaterial, string? nombreAlmacen)
        {
            var inventarios = await _unitOfWork.Inventarios.FindAsync(
                i => i.IdMaterial == idMaterial && i.IdAlmacen == idAlmacen);

            var inventario = inventarios.FirstOrDefault();

            if (inventario == null)
                throw new InvalidOperationException(
                    $"No existe registro de inventario para '{nombreMaterial ?? $"material {idMaterial}"}' " +
                    $"en '{nombreAlmacen ?? $"almacén {idAlmacen}"}'. " +
                    "Debe darse de alta el inventario antes de mover existencias.");

            return inventario;
        }

        public async Task<IEnumerable<EntregaEPP>> GetEntregasAsync(int? idEmpleado = null)
        {
            if (idEmpleado.HasValue)
            {
                return await _unitOfWork.EntregasEPP.FindAsync(
                    filter: e => e.IdEmpleado == idEmpleado.Value,
                    includeProperties: IncludeProperties);
            }

            return await _unitOfWork.EntregasEPP.GetAllAsync(includeProperties: IncludeProperties);
        }

        public async Task<EntregaEPP?> GetEntregaByIdAsync(int id)
        {
            return await _unitOfWork.EntregasEPP.GetByIdAsync(id, includeProperties: IncludeProperties);
        }

        public async Task<IEnumerable<EntregaEPP>> GetEntregasByMaterialAsync(int idMaterial)
        {
            return await _unitOfWork.EntregasEPP.FindAsync(
                filter: e => e.CompraDetalle != null && e.CompraDetalle.IdMaterial == idMaterial,
                includeProperties: IncludeProperties);
        }

        public async Task<(EntregaEPP Entrega, string? Alerta)> CreateEntregaAsync(EntregaEPP entrega, string? userId = null)
        {
            var empleado = await _unitOfWork.Empleados.GetByIdAsync(entrega.IdEmpleado);
            if (empleado == null)
                throw new ArgumentException($"El empleado con ID {entrega.IdEmpleado} no existe.");

            var compra = await _unitOfWork.ComprasEPPDetalle.GetByIdAsync(entrega.IdCompraDetalle, includeProperties: "Material,Almacen");
            if (compra == null)
                throw new ArgumentException($"El lote con ID {entrega.IdCompraDetalle} no existe.");

            if (compra.CantidadDisponible < entrega.CantidadEntregada)
                throw new InvalidOperationException(
                    $"No hay suficiente cantidad disponible en el lote. " +
                    $"Disponible: {compra.CantidadDisponible}, Solicitado: {entrega.CantidadEntregada}");

            compra.CantidadDisponible -= entrega.CantidadEntregada;
            _unitOfWork.ComprasEPPDetalle.Update(compra);

            // Sin registro de inventario la salida descontaba el lote y dejaba la
            // existencia intacta, sin error ni aviso. Es preferible detener la entrega
            // a generar un descuadre invisible.
            var inventario = await GetInventarioObligatorioAsync(
                compra.IdMaterial, compra.IdAlmacen, compra.Material?.Nombre, compra.Almacen?.Nombre);

            var movimiento = await _movimientoService.RegistrarAsync(
                compra.IdMaterial, compra.IdAlmacen,
                TipoMovimientoInventario.Salida, -entrega.CantidadEntregada, userId,
                costoUnitario: compra.PrecioUnitario,
                idCompraDetalle: compra.IdCompraDetalle,
                documentoTipo: "EntregaEPP",
                observaciones: $"Entrega a empleado {empleado.NombreCompleto}");

            await _unitOfWork.EntregasEPP.AddAsync(entrega);
            await _unitOfWork.CompleteAsync();

            // El folio de la entrega sólo existe después de guardarla; se ata aquí
            // para que el kardex pueda regresar al documento que lo originó.
            movimiento.DocumentoId = entrega.IdEntrega;
            _unitOfWork.MovimientosInventario.Update(movimiento);
            await _unitOfWork.CompleteAsync();

            // *** Evaluar alertas de consumo ***
            try
            {
                await _alertaConsumoService.EvaluarEntregaAsync(entrega);
            }
            catch (Exception)
            {
                // Log pero no fallar la entrega por error en alertas
            }

            // *** Actualizar estadísticas mensuales ***
            try
            {
                var fechaEntrega = entrega.FechaEntrega;
                await _dashboardService.RecalcularEstadisticasEmpleadoAsync(
                    entrega.IdEmpleado,
                    fechaEntrega.Year,
                    fechaEntrega.Month);

                if (empleado.IdProyecto > 0)
                {
                    await _dashboardService.RecalcularEstadisticasProyectoAsync(
                        empleado.IdProyecto,
                        fechaEntrega.Year,
                        fechaEntrega.Month);
                }
            }
            catch (Exception)
            {
                // Log pero no fallar la entrega por error en estadísticas
            }

            string? alerta = null;
            if (inventario.Existencia <= inventario.StockMinimo)
            {
                var materialNombre = compra.Material?.Nombre ?? "Material";
                var almacenNombre = compra.Almacen?.Nombre ?? "Almacén";

                if (inventario.Existencia <= 0)
                {
                    alerta = $"🚨 ALERTA CRÍTICA: El stock de '{materialNombre}' en '{almacenNombre}' se ha AGOTADO.";
                }
                else
                {
                    alerta = $"⚠️ ALERTA: El stock de '{materialNombre}' en '{almacenNombre}' ha llegado al mínimo. " +
                            $"Existencia actual: {inventario.Existencia}, Stock mínimo: {inventario.StockMinimo}";
                }
            }

            return (entrega, alerta);
        }

        public async Task<(bool Success, string? Alerta)> UpdateEntregaAsync(EntregaEPP entrega, string? userId = null)
        {
            var entregaExistente = await _unitOfWork.EntregasEPP.GetByIdAsync(entrega.IdEntrega, includeProperties: "CompraDetalle");
            if (entregaExistente == null)
                return (false, null);

            decimal diferencia = entrega.CantidadEntregada - entregaExistente.CantidadEntregada;

            if (diferencia != 0)
            {
                var compra = await _unitOfWork.ComprasEPPDetalle.GetByIdAsync(entregaExistente.IdCompraDetalle, includeProperties: "Material,Almacen");
                if (compra == null)
                    return (false, null);

                if (diferencia > 0 && compra.CantidadDisponible < diferencia)
                    throw new InvalidOperationException(
                        $"No hay suficiente cantidad disponible en el lote para aumentar la entrega. " +
                        $"Disponible: {compra.CantidadDisponible}, Adicional solicitado: {diferencia}");

                compra.CantidadDisponible -= diferencia;
                _unitOfWork.ComprasEPPDetalle.Update(compra);

                await GetInventarioObligatorioAsync(
                    compra.IdMaterial, compra.IdAlmacen, compra.Material?.Nombre, compra.Almacen?.Nombre);

                // Corregir la cantidad entregada es sacar un poco más o devolver un
                // poco: cada corrección deja su propio renglón en el kardex.
                await _movimientoService.RegistrarAsync(
                    compra.IdMaterial, compra.IdAlmacen,
                    diferencia > 0 ? TipoMovimientoInventario.Salida : TipoMovimientoInventario.Devolucion,
                    -diferencia, userId,
                    costoUnitario: compra.PrecioUnitario,
                    idCompraDetalle: compra.IdCompraDetalle,
                    documentoTipo: "EntregaEPP",
                    documentoId: entregaExistente.IdEntrega,
                    observaciones: $"Corrección de la entrega: de {entregaExistente.CantidadEntregada} a {entrega.CantidadEntregada}.");
            }

            entregaExistente.FechaEntrega = entrega.FechaEntrega;
            entregaExistente.CantidadEntregada = entrega.CantidadEntregada;
            entregaExistente.TallaEntregada = entrega.TallaEntregada;
            entregaExistente.Observaciones = entrega.Observaciones;

            _unitOfWork.EntregasEPP.Update(entregaExistente);
            var result = await _unitOfWork.CompleteAsync() > 0;

            string? alerta = null;
            if (diferencia > 0)
            {
                var compraActualizada = await _unitOfWork.ComprasEPPDetalle.GetByIdAsync(entregaExistente.IdCompraDetalle, includeProperties: "Material,Almacen");
                var inventarios = await _unitOfWork.Inventarios.FindAsync(
                    i => i.IdMaterial == compraActualizada!.IdMaterial && i.IdAlmacen == compraActualizada.IdAlmacen);
                var inventario = inventarios.FirstOrDefault();

                if (inventario != null && inventario.Existencia <= inventario.StockMinimo)
                {
                    alerta = $"⚠️ ALERTA: Stock bajo de '{compraActualizada!.Material?.Nombre}' en '{compraActualizada.Almacen?.Nombre}'.";
                }
            }

            // *** Actualizar estadísticas si hubo cambios ***
            if (result && diferencia != 0)
            {
                try
                {
                    var empleadoStats = await _unitOfWork.Empleados.GetByIdAsync(entregaExistente.IdEmpleado);
                    var fechaEntrega = entregaExistente.FechaEntrega;

                    await _dashboardService.RecalcularEstadisticasEmpleadoAsync(
                        entregaExistente.IdEmpleado,
                        fechaEntrega.Year,
                        fechaEntrega.Month);

                    if (empleadoStats?.IdProyecto > 0)
                    {
                        await _dashboardService.RecalcularEstadisticasProyectoAsync(
                            empleadoStats.IdProyecto,
                            fechaEntrega.Year,
                            fechaEntrega.Month);
                    }
                }
                catch (Exception)
                {
                    // Log pero no fallar
                }
            }

            return (result, alerta);
        }

        public async Task<bool> DeleteEntregaAsync(int id, string? userId = null)
        {
            var entrega = await _unitOfWork.EntregasEPP.GetByIdAsync(id);
            if (entrega == null)
                return false;

            var compra = await _unitOfWork.ComprasEPPDetalle.GetByIdAsync(entrega.IdCompraDetalle, includeProperties: "Material,Almacen");
            if (compra != null)
            {
                compra.CantidadDisponible += entrega.CantidadEntregada;
                _unitOfWork.ComprasEPPDetalle.Update(compra);

                await GetInventarioObligatorioAsync(
                    compra.IdMaterial, compra.IdAlmacen, compra.Material?.Nombre, compra.Almacen?.Nombre);

                await _movimientoService.RegistrarAsync(
                    compra.IdMaterial, compra.IdAlmacen,
                    TipoMovimientoInventario.Devolucion, entrega.CantidadEntregada, userId,
                    costoUnitario: compra.PrecioUnitario,
                    idCompraDetalle: compra.IdCompraDetalle,
                    documentoTipo: "EntregaEPP",
                    documentoId: entrega.IdEntrega,
                    observaciones: "La entrega se eliminó: el material regresa al almacén.");
            }

            // Guardar datos antes de eliminar
            var idEmpleado = entrega.IdEmpleado;
            var fechaEntrega = entrega.FechaEntrega;
            var empleadoData = await _unitOfWork.Empleados.GetByIdAsync(idEmpleado);
            var idProyecto = empleadoData?.IdProyecto;

            _unitOfWork.EntregasEPP.Remove(entrega);
            var result = await _unitOfWork.CompleteAsync() > 0;

            // Actualizar estadísticas después de eliminar
            if (result)
            {
                try
                {
                    await _dashboardService.RecalcularEstadisticasEmpleadoAsync(
                        idEmpleado,
                        fechaEntrega.Year,
                        fechaEntrega.Month);

                    if (idProyecto.HasValue)
                    {
                        await _dashboardService.RecalcularEstadisticasProyectoAsync(
                            idProyecto.Value,
                            fechaEntrega.Year,
                            fechaEntrega.Month);
                    }
                }
                catch (Exception)
                {
                    // Log pero no fallar
                }
            }

            return result;
        }
    }
}