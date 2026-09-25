using VH.Services.DTOs;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    public class DevolucionEPPService : IDevolucionEPPService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMovimientoInventarioService _movimientoService;

        private const string IncludeEntrega =
            "Empleado.Proyecto,CompraDetalle.Material.UnidadMedida,CompraDetalle.Almacen";

        public DevolucionEPPService(IUnitOfWork unitOfWork, IMovimientoInventarioService movimientoService)
        {
            _unitOfWork = unitOfWork;
            _movimientoService = movimientoService;
        }

        public async Task<IEnumerable<PrestamoDto>> GetPrestadosAsync(
            int? idEmpleado = null, int? idAlmacen = null)
        {
            // Sólo lo retornable: unos guantes entregados no se esperan de vuelta.
            var entregas = (await _unitOfWork.EntregasEPP.FindAsync(
                    e => (!idEmpleado.HasValue || e.IdEmpleado == idEmpleado.Value),
                    includeProperties: IncludeEntrega))
                .Where(e => e.CompraDetalle?.Material?.EsRetornable == true)
                // Sólo lo que tiene quien lo devuelva: el consumo cargado a la obra
                // se gastó ahí, no hay nadie a quien pedírselo de vuelta.
                .Where(e => e.IdEmpleado.HasValue)
                .Where(e => !idAlmacen.HasValue || e.CompraDetalle!.IdAlmacen == idAlmacen.Value)
                .ToList();

            if (entregas.Count == 0) return new List<PrestamoDto>();

            var ids = entregas.Select(e => e.IdEntrega).ToList();

            var devueltas = (await _unitOfWork.DevolucionesEPP.FindAsync(d => ids.Contains(d.IdEntrega)))
                .GroupBy(d => d.IdEntrega)
                .ToDictionary(g => g.Key, g => g.Sum(d => d.Cantidad));

            var prestados = entregas
                .Select(e =>
                {
                    devueltas.TryGetValue(e.IdEntrega, out var devuelta);

                    return new PrestamoDto
                    {
                        IdEntrega = e.IdEntrega,
                        FechaEntrega = e.FechaEntrega,
                        IdEmpleado = e.IdEmpleado!.Value,
                        NombreEmpleado = e.Empleado?.NombreCompleto ?? "",
                        NumeroNomina = e.Empleado?.NumeroNomina ?? "",
                        NombreProyecto = e.Empleado?.Proyecto?.Nombre ?? "",
                        IdMaterial = e.CompraDetalle?.IdMaterial ?? 0,
                        NombreMaterial = e.CompraDetalle?.Material?.Nombre ?? "",
                        UnidadMedida = e.CompraDetalle?.Material?.UnidadMedida?.Abreviatura ?? "",
                        Talla = e.TallaEntregada,
                        IdCompraDetalle = e.IdCompraDetalle,
                        LoteProveedor = e.CompraDetalle?.LoteProveedor,
                        IdAlmacen = e.CompraDetalle?.IdAlmacen ?? 0,
                        NombreAlmacen = e.CompraDetalle?.Almacen?.Nombre ?? "",
                        CantidadEntregada = e.CantidadEntregada,
                        CantidadDevuelta = devuelta
                    };
                })
                .Where(p => p.Pendiente > 0)
                .OrderByDescending(p => p.DiasFuera)
                .ThenBy(p => p.NombreEmpleado)
                .ToList();

            return prestados;
        }

        public async Task<ResultadoPaginado<DevolucionEPP>> GetHistorialPaginadoAsync(
            ConsultaPaginada consulta, int? idEmpleado = null)
        {
            var texto = consulta.TextoLimpio;

            return await _unitOfWork.DevolucionesEPP.GetPaginadoAsync(
                consulta,
                filtro: d =>
                    (idEmpleado == null || (d.Entrega != null && d.Entrega.IdEmpleado == idEmpleado)) &&
                    (texto == null ||
                     (d.Observaciones != null && d.Observaciones.Contains(texto)) ||
                     (d.Entrega != null && d.Entrega.Empleado != null &&
                      (d.Entrega.Empleado.Nombre.Contains(texto) ||
                       d.Entrega.Empleado.ApellidoPaterno.Contains(texto) ||
                       d.Entrega.Empleado.NumeroNomina.Contains(texto))) ||
                     (d.Entrega != null && d.Entrega.CompraDetalle != null &&
                      d.Entrega.CompraDetalle.Material != null &&
                      d.Entrega.CompraDetalle.Material.Nombre.Contains(texto))),
                orden: q => q.OrderByDescending(d => d.FechaDevolucion).ThenByDescending(d => d.IdDevolucion),
                includeProperties: "UsuarioRecibe,Entrega.Empleado,Entrega.CompraDetalle.Material.UnidadMedida");
        }

        public async Task<IEnumerable<DevolucionEPP>> GetDevolucionesAsync(int? idEmpleado = null)
        {
            var devoluciones = await _unitOfWork.DevolucionesEPP.GetAllAsync(
                includeProperties: "UsuarioRecibe,Entrega.Empleado,Entrega.CompraDetalle.Material.UnidadMedida");

            var lista = devoluciones.ToList();

            if (idEmpleado.HasValue)
                lista = lista.Where(d => d.Entrega?.IdEmpleado == idEmpleado.Value).ToList();

            return lista.OrderByDescending(d => d.FechaDevolucion).ToList();
        }

        public async Task<(DevolucionEPP Devolucion, string? Aviso)> RegistrarAsync(
            RegistrarDevolucionRequestDto dto, string userId)
        {
            var entrega = await _unitOfWork.EntregasEPP.GetByIdAsync(
                dto.IdEntrega, includeProperties: "Empleado,CompraDetalle.Material");

            if (entrega == null)
                throw new ArgumentException($"La entrega {dto.IdEntrega} no existe.");

            var material = entrega.CompraDetalle?.Material;

            if (material?.EsRetornable != true)
                throw new InvalidOperationException(
                    $"'{material?.Nombre ?? "Ese material"}' no es retornable: no se espera de vuelta. " +
                    "Si debería serlo, márquelo en el catálogo de materiales.");

            if (dto.Cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0.");

            var yaDevuelto = (await _unitOfWork.DevolucionesEPP.FindAsync(d => d.IdEntrega == dto.IdEntrega))
                .Sum(d => d.Cantidad);

            var pendiente = entrega.CantidadEntregada - yaDevuelto;

            if (dto.Cantidad > pendiente)
                throw new InvalidOperationException(
                    $"Sólo quedan {pendiente:0.##} sin devolver de esta entrega " +
                    $"(se entregaron {entrega.CantidadEntregada:0.##} y ya volvieron {yaDevuelto:0.##}).");

            if (dto.Estado != EstadoDevolucion.Buena && string.IsNullOrWhiteSpace(dto.Observaciones))
                throw new ArgumentException(
                    "Cuando algo vuelve dañado o no vuelve, hay que explicar qué pasó.");

            var transaccionPropia = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var devolucion = new DevolucionEPP
                {
                    IdEntrega = dto.IdEntrega,
                    Cantidad = dto.Cantidad,
                    FechaDevolucion = DateTime.Now,
                    IdUsuarioRecibe = userId,
                    Estado = dto.Estado,
                    Observaciones = dto.Observaciones
                };

                await _unitOfWork.DevolucionesEPP.AddAsync(devolucion);

                string? aviso = null;

                if (devolucion.RegresaAlAlmacen)
                {
                    // Vuelve al mismo lote del que salió: conserva su costo, su
                    // factura y su lote de fábrica.
                    var lote = entrega.CompraDetalle!;
                    lote.CantidadDisponible += dto.Cantidad;
                    _unitOfWork.ComprasEPPDetalle.Update(lote);

                    await _movimientoService.RegistrarAsync(
                        lote.IdMaterial, lote.IdAlmacen,
                        TipoMovimientoInventario.Devolucion, dto.Cantidad, userId,
                        costoUnitario: lote.PrecioUnitario,
                        idCompraDetalle: lote.IdCompraDetalle,
                        documentoTipo: "DevolucionEPP",
                        documentoId: dto.IdEntrega,
                        observaciones: dto.Estado == EstadoDevolucion.Dañada
                            ? $"Devuelto dañado por {entrega.Empleado?.NombreCompleto}. {dto.Observaciones}"
                            : $"Devuelto por {entrega.Empleado?.NombreCompleto}");

                    if (dto.Estado == EstadoDevolucion.Dañada)
                        aviso = $"'{material.Nombre}' volvió al almacén marcado como dañado. " +
                                "Revíselo antes de volver a prestarlo, o dele de baja con una merma.";
                }
                else
                {
                    // Lo perdido salió del anaquel el día que se entregó: no hay
                    // nada que devolver al inventario, sólo que dejarlo asentado.
                    aviso = $"'{material.Nombre}' se dio por perdido. No regresa al inventario: " +
                            "salió del almacén cuando se entregó.";
                }

                await _unitOfWork.CompleteAsync();
                if (transaccionPropia) await _unitOfWork.CommitTransactionAsync();

                return (devolucion, aviso);
            }
            catch
            {
                if (transaccionPropia) await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
