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

        private const string IncludeCabecera =
            "Proveedor,Detalles.Material.UnidadMedida,Detalles.Almacen.Proyecto";

        private const string IncludeLote =
            "Compra.Proveedor,Material.UnidadMedida,Almacen.Proyecto";

        public async Task<IEnumerable<CompraEPP>> GetComprasAsync(
            int? idMaterial = null,
            int? idProveedor = null,
            int? idAlmacen = null)
        {
            // Material y almacén viven en el renglón: la compra entra si alguno
            // de sus renglones coincide.
            if (idMaterial.HasValue || idProveedor.HasValue || idAlmacen.HasValue)
            {
                return await _unitOfWork.ComprasEPP.FindAsync(
                    filter: c =>
                        (!idProveedor.HasValue || c.IdProveedor == idProveedor.Value) &&
                        (!idMaterial.HasValue || c.Detalles.Any(d => d.IdMaterial == idMaterial.Value)) &&
                        (!idAlmacen.HasValue || c.Detalles.Any(d => d.IdAlmacen == idAlmacen.Value)),
                    includeProperties: IncludeCabecera);
            }

            return await _unitOfWork.ComprasEPP.GetAllAsync(includeProperties: IncludeCabecera);
        }

        public async Task<CompraEPP?> GetCompraByIdAsync(int id)
        {
            return await _unitOfWork.ComprasEPP.GetByIdAsync(id, includeProperties: IncludeCabecera);
        }

        public async Task<CompraEPPDetalle?> GetLoteByIdAsync(int idCompraDetalle)
        {
            return await _unitOfWork.ComprasEPPDetalle.GetByIdAsync(idCompraDetalle, includeProperties: IncludeLote);
        }

        public async Task<IEnumerable<CompraEPPDetalle>> GetLotesDisponiblesAsync(int idMaterial, int idAlmacen)
        {
            var lotes = await _unitOfWork.ComprasEPPDetalle.FindAsync(
                filter: d => d.IdMaterial == idMaterial &&
                             d.IdAlmacen == idAlmacen &&
                             d.CantidadDisponible > 0,
                includeProperties: "Compra.Proveedor");

            // Lo que caduca antes sale primero; a igualdad, lo más antiguo.
            return lotes
                .OrderBy(d => d.FechaCaducidad ?? DateTime.MaxValue)
                .ThenBy(d => d.Compra?.FechaCompra ?? DateTime.MaxValue)
                .ThenBy(d => d.IdCompraDetalle)
                .ToList();
        }

        public async Task<(CompraEPP Compra, List<string> Alertas)> CreateCompraAsync(CompraEPP compra)
        {
            var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(compra.IdProveedor);
            if (proveedor == null)
                throw new ArgumentException($"El proveedor con ID {compra.IdProveedor} no existe.");

            if (compra.Detalles == null || compra.Detalles.Count == 0)
                throw new InvalidOperationException("La compra debe tener al menos un material.");

            // Validar todos los renglones antes de escribir nada.
            foreach (var detalle in compra.Detalles)
            {
                var material = await _unitOfWork.Materiales.GetByIdAsync(detalle.IdMaterial);
                if (material == null)
                    throw new ArgumentException($"El material con ID {detalle.IdMaterial} no existe.");

                var almacen = await _unitOfWork.Almacenes.GetByIdAsync(detalle.IdAlmacen);
                if (almacen == null)
                    throw new ArgumentException($"El almacén con ID {detalle.IdAlmacen} no existe.");

                if (detalle.Cantidad <= 0)
                    throw new ArgumentException($"La cantidad de '{material.Nombre}' debe ser mayor a 0.");

                if (detalle.PrecioUnitario <= 0)
                    throw new ArgumentException($"El precio unitario de '{material.Nombre}' debe ser mayor a 0.");
            }

            var alertas = new List<string>();

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Un lote nace completo: lo disponible es lo comprado.
                foreach (var detalle in compra.Detalles)
                    detalle.CantidadDisponible = detalle.Cantidad;

                compra.Subtotal = compra.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
                compra.Total = compra.Subtotal + compra.Iva;

                await _unitOfWork.ComprasEPP.AddAsync(compra);
                await _unitOfWork.CompleteAsync();

                foreach (var detalle in compra.Detalles)
                {
                    var material = await _unitOfWork.Materiales.GetByIdAsync(detalle.IdMaterial);
                    var almacen = await _unitOfWork.Almacenes.GetByIdAsync(detalle.IdAlmacen);

                    // Costo de referencia del material = último precio pagado.
                    if (material != null)
                    {
                        material.CostoUnitarioEstimado = detalle.PrecioUnitario;
                        _unitOfWork.Materiales.Update(material);
                    }

                    var (inventario, esNuevo) = await GetOrCreateInventarioAsync(detalle.IdMaterial, detalle.IdAlmacen);
                    inventario.Existencia += detalle.Cantidad;
                    inventario.FechaUltimoMovimiento = DateTime.Now;

                    if (!esNuevo)
                        _unitOfWork.Inventarios.Update(inventario);

                    if (inventario.StockMaximo > 0 && inventario.Existencia > inventario.StockMaximo)
                    {
                        alertas.Add(
                            $"El stock de '{material?.Nombre ?? "material"}' en " +
                            $"'{almacen?.Nombre ?? "almacén"}' excede el máximo. " +
                            $"Existencia: {inventario.Existencia}, máximo: {inventario.StockMaximo}");
                    }
                }

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return (compra, alertas);
        }

        public async Task<bool> UpdateCompraAsync(int idCompra, DateTime fechaCompra, string? numeroDocumento,
            string? uuidCFDI, decimal iva, string? observaciones)
        {
            var compra = await _unitOfWork.ComprasEPP.GetByIdAsync(idCompra, includeProperties: "Detalles");
            if (compra == null)
                return false;

            compra.FechaCompra = fechaCompra;
            compra.NumeroDocumento = numeroDocumento ?? string.Empty;
            compra.UuidCFDI = uuidCFDI;
            compra.Iva = iva;
            compra.Observaciones = observaciones ?? string.Empty;

            // El subtotal siempre se deriva de los renglones, que aquí no cambian.
            // Cambiar el precio de un lote ya consumido reescribiría el costo de
            // entregas de meses cerrados, así que esta operación no lo permite.
            compra.Subtotal = compra.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
            compra.Total = compra.Subtotal + compra.Iva;

            _unitOfWork.ComprasEPP.Update(compra);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> DeleteCompraAsync(int id)
        {
            var compra = await _unitOfWork.ComprasEPP.GetByIdAsync(id, includeProperties: "Detalles.Material");
            if (compra == null)
                return false;

            var consumido = compra.Detalles.FirstOrDefault(d => d.CantidadDisponible != d.Cantidad);
            if (consumido != null)
                throw new InvalidOperationException(
                    "No se puede cancelar la compra: ya se entregó material del lote de " +
                    $"'{consumido.Material?.Nombre ?? $"material {consumido.IdMaterial}"}'.");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var detalle in compra.Detalles)
                {
                    var inventarios = await _unitOfWork.Inventarios.FindAsync(
                        i => i.IdMaterial == detalle.IdMaterial && i.IdAlmacen == detalle.IdAlmacen);
                    var inventario = inventarios.FirstOrDefault();

                    if (inventario != null)
                    {
                        inventario.Existencia -= detalle.Cantidad;
                        inventario.FechaUltimoMovimiento = DateTime.Now;
                        _unitOfWork.Inventarios.Update(inventario);
                    }
                }

                // Los renglones caen con la cabecera por la relación en cascada.
                _unitOfWork.ComprasEPP.Remove(compra);
                var result = await _unitOfWork.CompleteAsync() > 0;

                await _unitOfWork.CommitTransactionAsync();
                return result;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<IEnumerable<CompraEPPDetalle>> GetHistorialPreciosAsync(int idMaterial, int? idProveedor = null)
        {
            var detalles = await _unitOfWork.ComprasEPPDetalle.FindAsync(
                filter: d => d.IdMaterial == idMaterial &&
                             (!idProveedor.HasValue || d.Compra!.IdProveedor == idProveedor.Value),
                includeProperties: "Compra.Proveedor,Material.UnidadMedida,Almacen");

            return detalles.OrderByDescending(d => d.Compra?.FechaCompra).ToList();
        }

        private async Task<(Inventario Inventario, bool EsNuevo)> GetOrCreateInventarioAsync(int idMaterial, int idAlmacen)
        {
            var inventarios = await _unitOfWork.Inventarios.FindAsync(
                i => i.IdMaterial == idMaterial && i.IdAlmacen == idAlmacen);

            var inventario = inventarios.FirstOrDefault();

            if (inventario != null)
                return (inventario, false);

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
            return (inventario, true);
        }
    }
}
