using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    public class CompraEPPService : ICompraEPPService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMovimientoInventarioService _movimientoService;

        public CompraEPPService(IUnitOfWork unitOfWork, IMovimientoInventarioService movimientoService)
        {
            _unitOfWork = unitOfWork;
            _movimientoService = movimientoService;
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

        public async Task<(CompraEPP Compra, List<string> Alertas)> CreateCompraAsync(CompraEPP compra, string? userId = null)
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

                    // La entrada al almacén pasa por el kardex: él suma la existencia
                    // y deja el renglón que la explica.
                    var movimiento = await _movimientoService.RegistrarAsync(
                        detalle.IdMaterial, detalle.IdAlmacen,
                        TipoMovimientoInventario.Entrada, detalle.Cantidad, userId,
                        costoUnitario: detalle.PrecioUnitario,
                        idCompraDetalle: detalle.IdCompraDetalle,
                        documentoTipo: "CompraEPP",
                        documentoId: compra.IdCompra,
                        documentoFolio: compra.NumeroDocumento,
                        observaciones: $"Compra a {proveedor.Nombre}");

                    var inventario = await BuscarInventarioAsync(detalle.IdMaterial, detalle.IdAlmacen);

                    if (inventario != null && inventario.StockMaximo > 0 &&
                        inventario.Existencia > inventario.StockMaximo)
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

        public async Task<bool> DeleteCompraAsync(int id, string? userId = null)
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
                    // La cancelación revierte la entrada, y el kardex conserva ambos
                    // renglones: el que sumó y el que devolvió.
                    await _movimientoService.RegistrarAsync(
                        detalle.IdMaterial, detalle.IdAlmacen,
                        TipoMovimientoInventario.CancelacionCompra, -detalle.Cantidad, userId,
                        costoUnitario: detalle.PrecioUnitario,
                        documentoTipo: "CompraEPP",
                        documentoId: compra.IdCompra,
                        documentoFolio: compra.NumeroDocumento,
                        observaciones: "Compra cancelada: se revierte la entrada.");
                }

                // El kardex sobrevive a la cancelación: los renglones que apuntaban
                // a estos lotes sueltan la referencia y conservan el documento, de
                // modo que la entrada y su reverso siguen contando la historia.
                var idsLote = compra.Detalles.Select(d => d.IdCompraDetalle).ToList();

                var movimientosDelLote = await _unitOfWork.MovimientosInventario.FindAsync(
                    m => m.IdCompraDetalle != null && idsLote.Contains(m.IdCompraDetalle.Value));

                foreach (var movimiento in movimientosDelLote)
                {
                    movimiento.IdCompraDetalle = null;
                    _unitOfWork.MovimientosInventario.Update(movimiento);
                }

                await _unitOfWork.CompleteAsync();

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

        private async Task<Inventario?> BuscarInventarioAsync(int idMaterial, int idAlmacen)
        {
            var inventarios = await _unitOfWork.Inventarios.FindAsync(
                i => i.IdMaterial == idMaterial && i.IdAlmacen == idAlmacen);

            return inventarios.FirstOrDefault();
        }
    }
}
