using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    public class EntregaEPPService : IEntregaEPPService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAlertaConsumoService _alertaConsumoService;

        public EntregaEPPService(IUnitOfWork unitOfWork, IAlertaConsumoService alertaConsumoService)
        {
            _unitOfWork = unitOfWork;
            _alertaConsumoService = alertaConsumoService;
        }

        private const string IncludeProperties = "Empleado.Proyecto,Compra.Material.UnidadMedida,Compra.Proveedor,Compra.Almacen";

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
                filter: e => e.Compra != null && e.Compra.IdMaterial == idMaterial,
                includeProperties: IncludeProperties);
        }

        public async Task<(EntregaEPP Entrega, string? Alerta)> CreateEntregaAsync(EntregaEPP entrega)
        {
            var empleado = await _unitOfWork.Empleados.GetByIdAsync(entrega.IdEmpleado);
            if (empleado == null)
                throw new ArgumentException($"El empleado con ID {entrega.IdEmpleado} no existe.");

            var compra = await _unitOfWork.ComprasEPP.GetByIdAsync(entrega.IdCompra, includeProperties: "Material,Almacen");
            if (compra == null)
                throw new ArgumentException($"El lote/compra con ID {entrega.IdCompra} no existe.");

            if (compra.CantidadDisponible < entrega.CantidadEntregada)
                throw new InvalidOperationException(
                    $"No hay suficiente cantidad disponible en el lote. " +
                    $"Disponible: {compra.CantidadDisponible}, Solicitado: {entrega.CantidadEntregada}");

            compra.CantidadDisponible -= entrega.CantidadEntregada;
            _unitOfWork.ComprasEPP.Update(compra);

            var inventarios = await _unitOfWork.Inventarios.FindAsync(
                i => i.IdMaterial == compra.IdMaterial && i.IdAlmacen == compra.IdAlmacen);
            var inventario = inventarios.FirstOrDefault();

            if (inventario != null)
            {
                inventario.Existencia -= entrega.CantidadEntregada;
                inventario.FechaUltimoMovimiento = DateTime.Now;
                _unitOfWork.Inventarios.Update(inventario);
            }

            await _unitOfWork.EntregasEPP.AddAsync(entrega);
            await _unitOfWork.CompleteAsync();

            // *** NUEVO: Evaluar alertas de consumo ***
            try
            {
                await _alertaConsumoService.EvaluarEntregaAsync(entrega);
            }
            catch (Exception)
            {
                // Log pero no fallar la entrega por error en alertas
            }

            string? alerta = null;
            if (inventario != null && inventario.Existencia <= inventario.StockMinimo)
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

        public async Task<(bool Success, string? Alerta)> UpdateEntregaAsync(EntregaEPP entrega)
        {
            var entregaExistente = await _unitOfWork.EntregasEPP.GetByIdAsync(entrega.IdEntrega, includeProperties: "Compra");
            if (entregaExistente == null)
                return (false, null);

            decimal diferencia = entrega.CantidadEntregada - entregaExistente.CantidadEntregada;

            if (diferencia != 0)
            {
                var compra = await _unitOfWork.ComprasEPP.GetByIdAsync(entregaExistente.IdCompra, includeProperties: "Material,Almacen");
                if (compra == null)
                    return (false, null);

                if (diferencia > 0 && compra.CantidadDisponible < diferencia)
                    throw new InvalidOperationException(
                        $"No hay suficiente cantidad disponible en el lote para aumentar la entrega. " +
                        $"Disponible: {compra.CantidadDisponible}, Adicional solicitado: {diferencia}");

                compra.CantidadDisponible -= diferencia;
                _unitOfWork.ComprasEPP.Update(compra);

                var inventarios = await _unitOfWork.Inventarios.FindAsync(
                    i => i.IdMaterial == compra.IdMaterial && i.IdAlmacen == compra.IdAlmacen);
                var inventario = inventarios.FirstOrDefault();

                if (inventario != null)
                {
                    inventario.Existencia -= diferencia;
                    inventario.FechaUltimoMovimiento = DateTime.Now;
                    _unitOfWork.Inventarios.Update(inventario);
                }
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
                var compraActualizada = await _unitOfWork.ComprasEPP.GetByIdAsync(entregaExistente.IdCompra, includeProperties: "Material,Almacen");
                var inventarios = await _unitOfWork.Inventarios.FindAsync(
                    i => i.IdMaterial == compraActualizada!.IdMaterial && i.IdAlmacen == compraActualizada.IdAlmacen);
                var inventario = inventarios.FirstOrDefault();

                if (inventario != null && inventario.Existencia <= inventario.StockMinimo)
                {
                    alerta = $"⚠️ ALERTA: Stock bajo de '{compraActualizada!.Material?.Nombre}' en '{compraActualizada.Almacen?.Nombre}'.";
                }
            }

            return (result, alerta);
        }

        public async Task<bool> DeleteEntregaAsync(int id)
        {
            var entrega = await _unitOfWork.EntregasEPP.GetByIdAsync(id);
            if (entrega == null)
                return false;

            var compra = await _unitOfWork.ComprasEPP.GetByIdAsync(entrega.IdCompra);
            if (compra != null)
            {
                compra.CantidadDisponible += entrega.CantidadEntregada;
                _unitOfWork.ComprasEPP.Update(compra);

                var inventarios = await _unitOfWork.Inventarios.FindAsync(
                    i => i.IdMaterial == compra.IdMaterial && i.IdAlmacen == compra.IdAlmacen);
                var inventario = inventarios.FirstOrDefault();

                if (inventario != null)
                {
                    inventario.Existencia += entrega.CantidadEntregada;
                    inventario.FechaUltimoMovimiento = DateTime.Now;
                    _unitOfWork.Inventarios.Update(inventario);
                }
            }

            _unitOfWork.EntregasEPP.Remove(entrega);
            return await _unitOfWork.CompleteAsync() > 0;
        }
    }
}