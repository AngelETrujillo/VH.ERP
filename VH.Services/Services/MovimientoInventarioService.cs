using VH.Services.DTOs;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    public class MovimientoInventarioService : IMovimientoInventarioService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MovimientoInventarioService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<MovimientoInventario> RegistrarAsync(
            int idMaterial,
            int idAlmacen,
            TipoMovimientoInventario tipo,
            decimal cantidad,
            string? userId,
            decimal? costoUnitario = null,
            int? idCompraDetalle = null,
            string? documentoTipo = null,
            int? documentoId = null,
            string? documentoFolio = null,
            string? observaciones = null)
        {
            if (cantidad == 0)
                throw new ArgumentException("Un movimiento de cero no explica nada; no se registra.");

            var afectaExistencia = tipo != TipoMovimientoInventario.Reserva
                                   && tipo != TipoMovimientoInventario.LiberacionReserva;

            var inventario = await BuscarInventarioAsync(idMaterial, idAlmacen);

            // Una entrada puede crear el registro de inventario; una salida no:
            // sacar de un almacén donde el material nunca entró es un error.
            if (inventario == null)
            {
                if (!afectaExistencia || cantidad < 0)
                    throw new InvalidOperationException(
                        $"No existe registro de inventario para el material {idMaterial} " +
                        $"en el almacén {idAlmacen}. No se puede registrar el movimiento.");

                inventario = new Inventario
                {
                    IdMaterial = idMaterial,
                    IdAlmacen = idAlmacen,
                    Existencia = 0,
                    Comprometido = 0,
                    StockMinimo = 0,
                    StockMaximo = 0,
                    UbicacionPasillo = string.Empty,
                    FechaUltimoMovimiento = DateTime.Now
                };

                // Se persiste enseguida para que exista una sola fila por
                // material/almacén: si la misma compra trae dos renglones del mismo
                // material, el segundo debe encontrar el registro del primero.
                await _unitOfWork.Inventarios.AddAsync(inventario);
                await _unitOfWork.CompleteAsync();
            }

            if (afectaExistencia)
            {
                inventario.Existencia += cantidad;
                inventario.FechaUltimoMovimiento = DateTime.Now;
                _unitOfWork.Inventarios.Update(inventario);
            }

            var movimiento = new MovimientoInventario
            {
                IdMaterial = idMaterial,
                IdAlmacen = idAlmacen,
                Tipo = tipo,
                Cantidad = cantidad,
                CostoUnitario = costoUnitario,
                IdCompraDetalle = idCompraDetalle,
                DocumentoTipo = documentoTipo,
                DocumentoId = documentoId,
                DocumentoFolio = documentoFolio,
                Observaciones = observaciones,
                Fecha = DateTime.Now,
                IdUsuario = userId,
                SaldoResultante = inventario.Existencia
            };

            await _unitOfWork.MovimientosInventario.AddAsync(movimiento);
            return movimiento;
        }

        public async Task<IEnumerable<MovimientoInventario>> GetKardexAsync(
            int idMaterial, int idAlmacen, DateTime? desde = null, DateTime? hasta = null)
        {
            var movimientos = await _unitOfWork.MovimientosInventario.FindAsync(
                m => m.IdMaterial == idMaterial
                     && m.IdAlmacen == idAlmacen
                     && (!desde.HasValue || m.Fecha >= desde.Value)
                     && (!hasta.HasValue || m.Fecha <= hasta.Value),
                includeProperties: "Material.UnidadMedida,Almacen,Usuario");

            return movimientos
                .OrderBy(m => m.Fecha)
                .ThenBy(m => m.IdMovimiento)
                .ToList();
        }

        public async Task<decimal> GetSaldoAFechaAsync(int idMaterial, int idAlmacen, DateTime fecha)
        {
            // El saldo a una fecha es la suma de todo lo que movió existencia hasta
            // ese momento. Las reservas no cuentan: apartan, no sacan del anaquel.
            var movimientos = await _unitOfWork.MovimientosInventario.FindAsync(
                m => m.IdMaterial == idMaterial
                     && m.IdAlmacen == idAlmacen
                     && m.Fecha <= fecha
                     && m.Tipo != TipoMovimientoInventario.Reserva
                     && m.Tipo != TipoMovimientoInventario.LiberacionReserva);

            return movimientos.Sum(m => m.Cantidad);
        }

        public async Task<IEnumerable<DescuadreDto>> ReconciliarAsync()
        {
            var inventarios = await _unitOfWork.Inventarios.GetAllAsync(
                includeProperties: "Material,Almacen");

            var movimientos = await _unitOfWork.MovimientosInventario.GetAllAsync();

            var saldos = movimientos
                .Where(m => m.Tipo != TipoMovimientoInventario.Reserva
                            && m.Tipo != TipoMovimientoInventario.LiberacionReserva)
                .GroupBy(m => new { m.IdMaterial, m.IdAlmacen })
                .ToDictionary(g => (g.Key.IdMaterial, g.Key.IdAlmacen), g => g.Sum(m => m.Cantidad));

            var descuadres = new List<DescuadreDto>();

            foreach (var inv in inventarios)
            {
                saldos.TryGetValue((inv.IdMaterial, inv.IdAlmacen), out var saldoKardex);

                if (inv.Existencia == saldoKardex) continue;

                descuadres.Add(new DescuadreDto
                {
                    IdInventario = inv.IdInventario,
                    IdMaterial = inv.IdMaterial,
                    NombreMaterial = inv.Material?.Nombre ?? $"Material {inv.IdMaterial}",
                    IdAlmacen = inv.IdAlmacen,
                    NombreAlmacen = inv.Almacen?.Nombre ?? $"Almacén {inv.IdAlmacen}",
                    SaldoRegistrado = inv.Existencia,
                    SaldoKardex = saldoKardex
                });
            }

            return descuadres.OrderByDescending(d => Math.Abs(d.Diferencia)).ToList();
        }

        public async Task<MovimientoInventario> AjustarAsync(
            int idMaterial, int idAlmacen, decimal existenciaContada, string motivo, string? userId)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("Debe explicar el motivo del ajuste.");

            if (existenciaContada < 0)
                throw new ArgumentException("La existencia contada no puede ser negativa.");

            var inventario = await BuscarInventarioAsync(idMaterial, idAlmacen)
                ?? throw new InvalidOperationException(
                    "No existe registro de inventario para ese material en ese almacén.");

            var diferencia = existenciaContada - inventario.Existencia;
            if (diferencia == 0)
                throw new InvalidOperationException(
                    "El conteo coincide con la existencia registrada: no hay nada que ajustar.");

            // Un ajuste no puede dejar apartado más de lo que hay.
            if (existenciaContada < inventario.Comprometido)
                throw new InvalidOperationException(
                    $"El conteo ({existenciaContada}) es menor que lo ya apartado para requisiciones " +
                    $"({inventario.Comprometido}). Libere esas reservas antes de ajustar.");

            var movimiento = await RegistrarAsync(
                idMaterial, idAlmacen, TipoMovimientoInventario.Ajuste, diferencia, userId,
                documentoTipo: "Ajuste",
                observaciones: $"Conteo físico: {existenciaContada}. {motivo}");

            await _unitOfWork.CompleteAsync();
            return movimiento;
        }

        public async Task<(MovimientoInventario Salida, MovimientoInventario Entrada)> TraspasarAsync(
            int idMaterial, int idAlmacenOrigen, int idAlmacenDestino,
            decimal cantidad, string motivo, string? userId)
        {
            if (idAlmacenOrigen == idAlmacenDestino)
                throw new ArgumentException("El origen y el destino no pueden ser el mismo almacén.");

            if (cantidad <= 0)
                throw new ArgumentException("La cantidad a traspasar debe ser mayor a 0.");

            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("Debe indicar el motivo del traspaso.");

            var origen = await BuscarInventarioAsync(idMaterial, idAlmacenOrigen)
                ?? throw new InvalidOperationException("El material no tiene existencia en el almacén de origen.");

            // Sólo se traspasa lo que no está prometido a nadie.
            if (origen.Disponible < cantidad)
                throw new InvalidOperationException(
                    $"El almacén de origen sólo tiene {origen.Disponible} disponible " +
                    $"(existencia {origen.Existencia}, apartado {origen.Comprometido}).");

            var destino = await _unitOfWork.Almacenes.GetByIdAsync(idAlmacenDestino)
                ?? throw new ArgumentException($"El almacén {idAlmacenDestino} no existe.");

            var almacenOrigen = await _unitOfWork.Almacenes.GetByIdAsync(idAlmacenOrigen);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var salida = await RegistrarAsync(
                    idMaterial, idAlmacenOrigen, TipoMovimientoInventario.TraspasoSalida, -cantidad, userId,
                    documentoTipo: "Traspaso",
                    observaciones: $"Hacia {destino.Nombre}. {motivo}");

                var entrada = await RegistrarAsync(
                    idMaterial, idAlmacenDestino, TipoMovimientoInventario.TraspasoEntrada, cantidad, userId,
                    documentoTipo: "Traspaso",
                    observaciones: $"Desde {almacenOrigen?.Nombre ?? "otro almacén"}. {motivo}");

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return (salida, entrada);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<MovimientoInventario> RegistrarMermaAsync(
            int idMaterial, int idAlmacen, decimal cantidad, string motivo, string? userId)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0.");

            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("Debe explicar el motivo de la baja.");

            var inventario = await BuscarInventarioAsync(idMaterial, idAlmacen)
                ?? throw new InvalidOperationException("El material no tiene existencia en ese almacén.");

            if (inventario.Disponible < cantidad)
                throw new InvalidOperationException(
                    $"Sólo hay {inventario.Disponible} disponible " +
                    $"(existencia {inventario.Existencia}, apartado {inventario.Comprometido}).");

            var movimiento = await RegistrarAsync(
                idMaterial, idAlmacen, TipoMovimientoInventario.Merma, -cantidad, userId,
                documentoTipo: "Merma",
                observaciones: motivo);

            await _unitOfWork.CompleteAsync();
            return movimiento;
        }

        private async Task<Inventario?> BuscarInventarioAsync(int idMaterial, int idAlmacen)
        {
            var inventarios = await _unitOfWork.Inventarios.FindAsync(
                i => i.IdMaterial == idMaterial && i.IdAlmacen == idAlmacen);
            return inventarios.FirstOrDefault();
        }
    }
}
