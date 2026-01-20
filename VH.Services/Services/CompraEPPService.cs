using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    public class CompraEPPService : ICompraEPPService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompraEPPService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private const string IncludeProperties = "Material.UnidadMedida,Proveedor,Almacen.Proyecto";

        public async Task<IEnumerable<CompraEPP>> GetComprasAsync(
            int? idMaterial = null,
            int? idProveedor = null,
            int? idAlmacen = null)
        {
            if (idMaterial.HasValue || idProveedor.HasValue || idAlmacen.HasValue)
            {
                return await _unitOfWork.ComprasEPP.FindAsync(
                    filter: c =>
                        (!idMaterial.HasValue || c.IdMaterial == idMaterial.Value) &&
                        (!idProveedor.HasValue || c.IdProveedor == idProveedor.Value) &&
                        (!idAlmacen.HasValue || c.IdAlmacen == idAlmacen.Value),
                    includeProperties: IncludeProperties);
            }

            return await _unitOfWork.ComprasEPP.GetAllAsync(includeProperties: IncludeProperties);
        }

        public async Task<CompraEPP?> GetCompraByIdAsync(int id)
        {
            return await _unitOfWork.ComprasEPP.GetByIdAsync(id, includeProperties: IncludeProperties);
        }

        public async Task<IEnumerable<CompraEPP>> GetLotesDisponiblesAsync(int idMaterial, int idAlmacen)
        {
            return await _unitOfWork.ComprasEPP.FindAsync(
                filter: c => c.IdMaterial == idMaterial &&
                            c.IdAlmacen == idAlmacen &&
                            c.CantidadDisponible > 0,
                includeProperties: "Proveedor");
        }

        public async Task<(CompraEPP Compra, string? Alerta)> CreateCompraAsync(CompraEPP compra)
        {
            var material = await _unitOfWork.MaterialesEPP.GetByIdAsync(compra.IdMaterial);
            if (material == null)
                throw new ArgumentException($"El material con ID {compra.IdMaterial} no existe.");

            var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(compra.IdProveedor);
            if (proveedor == null)
                throw new ArgumentException($"El proveedor con ID {compra.IdProveedor} no existe.");

            var almacen = await _unitOfWork.Almacenes.GetByIdAsync(compra.IdAlmacen);
            if (almacen == null)
                throw new ArgumentException($"El almacén con ID {compra.IdAlmacen} no existe.");

            compra.CantidadDisponible = compra.CantidadComprada;

            await _unitOfWork.ComprasEPP.AddAsync(compra);

            material.CostoUnitarioEstimado = compra.PrecioUnitario;
            _unitOfWork.MaterialesEPP.Update(material);

            // Buscar inventario existente
            var inventarios = await _unitOfWork.Inventarios.FindAsync(
                i => i.IdMaterial == compra.IdMaterial && i.IdAlmacen == compra.IdAlmacen);
            var inventario = inventarios.FirstOrDefault();

            bool esNuevoInventario = inventario == null;

            if (esNuevoInventario)
            {
                // Crear nuevo inventario
                inventario = new Inventario
                {
                    IdMaterial = compra.IdMaterial,
                    IdAlmacen = compra.IdAlmacen,
                    Existencia = compra.CantidadComprada,
                    StockMinimo = 0,
                    StockMaximo = 0,
                    UbicacionPasillo = string.Empty,
                    FechaUltimoMovimiento = DateTime.Now
                };
                await _unitOfWork.Inventarios.AddAsync(inventario);
            }
            else
            {
                // Actualizar inventario existente
                inventario.Existencia += compra.CantidadComprada;
                inventario.FechaUltimoMovimiento = DateTime.Now;
                _unitOfWork.Inventarios.Update(inventario);
            }

            await _unitOfWork.CompleteAsync();

            string? alerta = null;
            if (inventario.StockMaximo > 0 && inventario.Existencia > inventario.StockMaximo)
            {
                alerta = $"⚠️ ALERTA: El stock de '{material.Nombre}' en '{almacen.Nombre}' ha excedido el máximo. " +
                        $"Existencia actual: {inventario.Existencia}, Stock máximo: {inventario.StockMaximo}";
            }

            return (compra, alerta);
        }

        public async Task<bool> UpdateCompraAsync(CompraEPP compra)
        {
            var compraExistente = await _unitOfWork.ComprasEPP.GetByIdAsync(compra.IdCompra);
            if (compraExistente == null)
                return false;

            compraExistente.FechaCompra = compra.FechaCompra;
            compraExistente.PrecioUnitario = compra.PrecioUnitario;
            compraExistente.NumeroDocumento = compra.NumeroDocumento;
            compraExistente.Observaciones = compra.Observaciones;

            _unitOfWork.ComprasEPP.Update(compraExistente);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> DeleteCompraAsync(int id)
        {
            var compra = await _unitOfWork.ComprasEPP.GetByIdAsync(id);
            if (compra == null)
                return false;

            if (compra.CantidadDisponible != compra.CantidadComprada)
                throw new InvalidOperationException(
                    "No se puede eliminar la compra porque ya se han realizado entregas de este lote.");

            var inventario = await GetInventarioAsync(compra.IdMaterial, compra.IdAlmacen);
            if (inventario != null)
            {
                inventario.Existencia -= compra.CantidadComprada;
                inventario.FechaUltimoMovimiento = DateTime.Now;
                _unitOfWork.Inventarios.Update(inventario);
            }

            _unitOfWork.ComprasEPP.Remove(compra);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<IEnumerable<CompraEPP>> GetHistorialPreciosAsync(int idMaterial, int? idProveedor = null)
        {
            return await _unitOfWork.ComprasEPP.FindAsync(
                filter: c => c.IdMaterial == idMaterial &&
                            (!idProveedor.HasValue || c.IdProveedor == idProveedor.Value),
                includeProperties: "Proveedor");
        }

        private async Task<Inventario> GetOrCreateInventarioAsync(int idMaterial, int idAlmacen)
        {
            var inventarios = await _unitOfWork.Inventarios.FindAsync(
                i => i.IdMaterial == idMaterial && i.IdAlmacen == idAlmacen);

            var inventario = inventarios.FirstOrDefault();

            if (inventario == null)
            {
                inventario = new Inventario
                {
                    IdMaterial = idMaterial,
                    IdAlmacen = idAlmacen,
                    Existencia = 0,
                    StockMinimo = 0,
                    StockMaximo = 0,
                    UbicacionPasillo = string.Empty,
                    FechaUltimoMovimiento = DateTime.Now
                };
                await _unitOfWork.Inventarios.AddAsync(inventario);
            }

            return inventario;
        }

        private async Task<Inventario?> GetInventarioAsync(int idMaterial, int idAlmacen)
        {
            var inventarios = await _unitOfWork.Inventarios.FindAsync(
                i => i.IdMaterial == idMaterial && i.IdAlmacen == idAlmacen);
            return inventarios.FirstOrDefault();
        }
    }
}