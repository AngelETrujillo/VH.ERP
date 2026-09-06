using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    public class InventarioService : IInventarioService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InventarioService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private const string IncludeProperties = "Almacen.Proyecto,Material.UnidadMedida";

        public async Task<IEnumerable<Inventario>> GetAllInventariosAsync()
        {
            return await _unitOfWork.Inventarios.GetAllAsync(includeProperties: IncludeProperties);
        }

        public async Task<IEnumerable<Inventario>> GetInventariosByAlmacenAsync(int idAlmacen)
        {
            return await _unitOfWork.Inventarios.FindAsync(
                filter: i => i.IdAlmacen == idAlmacen,
                includeProperties: IncludeProperties);
        }

        public async Task<IEnumerable<Inventario>> GetInventariosByMaterialAsync(int idMaterial)
        {
            return await _unitOfWork.Inventarios.FindAsync(
                filter: i => i.IdMaterial == idMaterial,
                includeProperties: IncludeProperties);
        }

        public async Task<Inventario?> GetInventarioByIdAsync(int id)
        {
            return await _unitOfWork.Inventarios.GetByIdAsync(id, includeProperties: IncludeProperties);
        }

        public async Task<Inventario?> GetInventarioByMaterialAlmacenAsync(int idMaterial, int idAlmacen)
        {
            var inventarios = await _unitOfWork.Inventarios.FindAsync(
                filter: i => i.IdMaterial == idMaterial && i.IdAlmacen == idAlmacen,
                includeProperties: IncludeProperties);
            return inventarios.FirstOrDefault();
        }

        public async Task<Inventario> CreateOrUpdateInventarioAsync(Inventario inventario)
        {
            var existente = await GetInventarioByMaterialAlmacenAsync(inventario.IdMaterial, inventario.IdAlmacen);

            if (existente != null)
            {
                existente.StockMinimo = inventario.StockMinimo;
                existente.StockMaximo = inventario.StockMaximo;
                existente.UbicacionPasillo = inventario.UbicacionPasillo;
                _unitOfWork.Inventarios.Update(existente);
                await _unitOfWork.CompleteAsync();
                return existente;
            }

            var material = await _unitOfWork.Materiales.GetByIdAsync(inventario.IdMaterial);
            if (material == null)
                throw new ArgumentException($"El material con ID {inventario.IdMaterial} no existe.");

            var almacen = await _unitOfWork.Almacenes.GetByIdAsync(inventario.IdAlmacen);
            if (almacen == null)
                throw new ArgumentException($"El almacén con ID {inventario.IdAlmacen} no existe.");

            inventario.Existencia = await RecalcularExistenciaAsync(inventario.IdMaterial, inventario.IdAlmacen);
            inventario.FechaUltimoMovimiento = DateTime.Now;

            await _unitOfWork.Inventarios.AddAsync(inventario);
            await _unitOfWork.CompleteAsync();
            return inventario;
        }

        public async Task<bool> UpdateConfiguracionAsync(int id, decimal stockMinimo, decimal stockMaximo, string ubicacion)
        {
            var inventario = await _unitOfWork.Inventarios.GetByIdAsync(id);
            if (inventario == null)
                return false;

            if (stockMaximo > 0 && stockMinimo > stockMaximo)
                throw new ArgumentException("El stock mínimo no puede ser mayor que el stock máximo.");

            inventario.StockMinimo = stockMinimo;
            inventario.StockMaximo = stockMaximo;
            inventario.UbicacionPasillo = ubicacion;

            _unitOfWork.Inventarios.Update(inventario);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> DeleteInventarioAsync(int id)
        {
            var inventario = await _unitOfWork.Inventarios.GetByIdAsync(id);
            if (inventario == null)
                return false;

            var compras = await _unitOfWork.ComprasEPPDetalle.FindAsync(
                d => d.IdMaterial == inventario.IdMaterial && d.IdAlmacen == inventario.IdAlmacen);

            if (compras.Any())
                throw new InvalidOperationException(
                    "No se puede eliminar el registro de inventario porque existen compras asociadas.");

            _unitOfWork.Inventarios.Remove(inventario);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<decimal> GetStockGlobalMaterialAsync(int idMaterial)
        {
            var inventarios = await _unitOfWork.Inventarios.FindAsync(i => i.IdMaterial == idMaterial);
            return inventarios.Sum(i => i.Existencia);
        }

        public async Task<IEnumerable<Inventario>> GetInventariosConAlertasAsync()
        {
            var inventarios = await _unitOfWork.Inventarios.GetAllAsync(includeProperties: IncludeProperties);

            return inventarios.Where(i =>
                i.Existencia <= 0 ||
                i.Existencia <= i.StockMinimo ||
                (i.StockMaximo > 0 && i.Existencia >= i.StockMaximo));
        }

        public async Task<decimal> RecalcularExistenciaAsync(int idMaterial, int idAlmacen)
        {
            var compras = await _unitOfWork.ComprasEPPDetalle.FindAsync(
                d => d.IdMaterial == idMaterial && d.IdAlmacen == idAlmacen);

            return compras.Sum(d => d.CantidadDisponible);
        }

        // ===== RESERVA DE EXISTENCIA =====

        public async Task<decimal> GetDisponibleAsync(int idMaterial, int idAlmacen)
        {
            var inventario = await BuscarAsync(idMaterial, idAlmacen);
            return inventario?.Disponible ?? 0;
        }

        public async Task<bool> ReservarAsync(int idMaterial, int idAlmacen, decimal cantidad)
        {
            if (cantidad <= 0) return false;

            var inventario = await BuscarAsync(idMaterial, idAlmacen);

            // Sin registro de inventario no hay nada que apartar: el material
            // nunca ha entrado a ese almacén.
            if (inventario == null) return false;

            if (inventario.Disponible < cantidad) return false;

            inventario.Comprometido += cantidad;
            _unitOfWork.Inventarios.Update(inventario);
            return true;
        }

        public async Task LiberarReservaAsync(int idMaterial, int idAlmacen, decimal cantidad)
        {
            if (cantidad <= 0) return;

            var inventario = await BuscarAsync(idMaterial, idAlmacen);
            if (inventario == null) return;

            // Nunca por debajo de cero: un comprometido negativo prometería
            // más existencia de la que hay.
            inventario.Comprometido = Math.Max(0, inventario.Comprometido - cantidad);
            _unitOfWork.Inventarios.Update(inventario);
        }

        public Task ConsumirReservaAsync(int idMaterial, int idAlmacen, decimal cantidad)
        {
            // Consumir y liberar hacen lo mismo sobre el comprometido; se separan
            // porque en el kardex serán dos movimientos distintos.
            return LiberarReservaAsync(idMaterial, idAlmacen, cantidad);
        }

        private async Task<Inventario?> BuscarAsync(int idMaterial, int idAlmacen)
        {
            var inventarios = await _unitOfWork.Inventarios.FindAsync(
                i => i.IdMaterial == idMaterial && i.IdAlmacen == idAlmacen);
            return inventarios.FirstOrDefault();
        }
    }
}