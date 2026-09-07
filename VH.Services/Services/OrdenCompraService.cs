using VH.Services.DTOs;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    public class OrdenCompraService : IOrdenCompraService
    {
        private readonly IUnitOfWork _unitOfWork;

        private const string IncludeOrden =
            "Proveedor,UsuarioEmite,Detalles.Material.UnidadMedida,Detalles.AlmacenDestino";

        public OrdenCompraService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<FaltanteDto>> GetFaltantesAsync(int? idAlmacen = null)
        {
            // Sólo lo autorizado que espera compra. Un renglón que ya entró a una
            // orden está en EnOrdenCompra y no vuelve a aparecer aquí: ahí está el
            // candado contra pedir dos veces lo mismo.
            var renglones = (await _unitOfWork.RequisicionesEPPDetalle.FindAsync(
                d => d.EstadoRenglon == EstadoRenglonRequisicion.PorComprar,
                includeProperties: "Requisicion.Almacen.Proyecto,Material.UnidadMedida,EmpleadoDestino"))
                .Where(d => d.Requisicion != null)
                .Where(d => !idAlmacen.HasValue || d.Requisicion!.IdAlmacen == idAlmacen.Value)
                .ToList();

            if (renglones.Count == 0)
                return new List<FaltanteDto>();

            var inventarios = (await _unitOfWork.Inventarios.GetAllAsync()).ToList();

            // Lo pedido a proveedores y aún sin recibir, descontando lo que ya está
            // prometido a alguien.
            //
            // Sin ese descuento, dos cascos que vienen en camino para Juan y Victor
            // "cubrirían" también la petición de un tercero, que se quedaría
            // esperando un material que nadie va a comprar. Es el mismo problema que
            // el comprometido resuelve sobre la existencia, aplicado al tránsito.
            var renglonesOrden = (await _unitOfWork.OrdenesCompraDetalle.FindAsync(
                    d => d.CantidadRecibida < d.CantidadPedida,
                    includeProperties: "OrdenCompra,Coberturas"))
                .Where(d => d.OrdenCompra != null && d.OrdenCompra.Estado != EstadoOrdenCompra.Cancelada)
                .ToList();

            var enTransito = renglonesOrden
                .GroupBy(d => new { d.IdMaterial, d.IdAlmacenDestino })
                .ToDictionary(
                    g => (g.Key.IdMaterial, g.Key.IdAlmacenDestino),
                    g => g.Sum(d => Math.Max(0,
                        (d.CantidadPedida - d.CantidadRecibida) - (d.Coberturas?.Sum(c => c.Cantidad) ?? 0))));

            var faltantes = new List<FaltanteDto>();

            foreach (var grupo in renglones.GroupBy(d => new { d.IdMaterial, Almacen = d.Requisicion!.IdAlmacen }))
            {
                var inventario = inventarios.FirstOrDefault(
                    i => i.IdMaterial == grupo.Key.IdMaterial && i.IdAlmacen == grupo.Key.Almacen);

                var existencia = inventario?.Existencia ?? 0;
                var comprometido = inventario?.Comprometido ?? 0;
                var disponible = existencia - comprometido;

                enTransito.TryGetValue((grupo.Key.IdMaterial, grupo.Key.Almacen), out var transito);

                var demanda = grupo.Sum(d => d.CantidadSolicitada);

                // Lo que hay que comprar. Si sale cero o menos, algo cambió desde la
                // autorización (llegó material o se liberó una reserva) y ya no hace
                // falta pedirlo, aunque el renglón siga marcado por comprar.
                var faltante = demanda - Math.Max(0, disponible) - transito;
                if (faltante <= 0) continue;

                var primero = grupo.First();
                var ultimaCompra = await GetUltimaCompraAsync(grupo.Key.IdMaterial);

                faltantes.Add(new FaltanteDto
                {
                    IdMaterial = grupo.Key.IdMaterial,
                    NombreMaterial = primero.Material?.Nombre ?? $"Material {grupo.Key.IdMaterial}",
                    UnidadMedida = primero.Material?.UnidadMedida?.Abreviatura ?? "",
                    IdAlmacen = grupo.Key.Almacen,
                    NombreAlmacen = primero.Requisicion?.Almacen?.Nombre ?? "",
                    NombreProyecto = primero.Requisicion?.Almacen?.Proyecto?.Nombre ?? "",
                    Demanda = demanda,
                    Existencia = existencia,
                    Comprometido = comprometido,
                    EnTransito = transito,
                    Faltante = faltante,
                    UltimoPrecio = ultimaCompra?.PrecioUnitario ?? primero.Material?.CostoUnitarioEstimado ?? 0,
                    IdUltimoProveedor = ultimaCompra?.Compra?.IdProveedor,
                    UltimoProveedor = ultimaCompra?.Compra?.Proveedor?.Nombre,
                    Renglones = grupo
                        .OrderBy(d => d.Requisicion!.FechaSolicitud)
                        .ThenBy(d => d.IdRequisicionDetalle)
                        .Select(d => new FaltanteRenglonDto
                        {
                            IdRequisicionDetalle = d.IdRequisicionDetalle,
                            IdRequisicion = d.IdRequisicion,
                            NumeroRequisicion = d.Requisicion?.NumeroRequisicion ?? "",
                            Cantidad = d.CantidadSolicitada,
                            IdEmpleadoDestino = d.IdEmpleadoDestino,
                            NombreEmpleado = d.EmpleadoDestino?.NombreCompleto ?? "",
                            FechaSolicitud = d.Requisicion?.FechaSolicitud ?? DateTime.Now,
                            FechaRequerida = d.Requisicion?.FechaRequerida
                        })
                        .ToList()
                });
            }

            // Lo que lleva más tiempo esperando, primero.
            return faltantes
                .OrderByDescending(f => f.DiasEsperaMaximo)
                .ThenBy(f => f.NombreMaterial)
                .ToList();
        }

        public async Task<IEnumerable<OrdenCompra>> GetOrdenesAsync(EstadoOrdenCompra? estado = null)
        {
            if (estado.HasValue)
            {
                return await _unitOfWork.OrdenesCompra.FindAsync(
                    o => o.Estado == estado.Value, includeProperties: IncludeOrden);
            }

            return await _unitOfWork.OrdenesCompra.GetAllAsync(includeProperties: IncludeOrden);
        }

        public async Task<OrdenCompra?> GetOrdenByIdAsync(int id)
        {
            var orden = await _unitOfWork.OrdenesCompra.GetByIdAsync(id, includeProperties: IncludeOrden);
            if (orden == null) return null;

            // Las coberturas en consulta aparte, para no acumular caminos de include
            // en una sola consulta.
            await _unitOfWork.RequisicionesCobertura.FindAsync(
                c => c.OrdenCompraDetalle != null && c.OrdenCompraDetalle.IdOrdenCompra == id,
                includeProperties: "RequisicionDetalle.Requisicion,RequisicionDetalle.EmpleadoDestino");

            return orden;
        }

        public async Task<OrdenCompra> GenerarAsync(GenerarOrdenCompraRequestDto dto, string userId)
        {
            var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(dto.IdProveedor);
            if (proveedor == null)
                throw new ArgumentException($"El proveedor con ID {dto.IdProveedor} no existe.");

            if (dto.Lineas == null || dto.Lineas.Count == 0)
                throw new InvalidOperationException("La orden debe incluir al menos un material.");

            // Validar todo antes de escribir.
            foreach (var linea in dto.Lineas)
            {
                var material = await _unitOfWork.Materiales.GetByIdAsync(linea.IdMaterial);
                if (material == null)
                    throw new ArgumentException($"El material con ID {linea.IdMaterial} no existe.");

                var almacen = await _unitOfWork.Almacenes.GetByIdAsync(linea.IdAlmacenDestino);
                if (almacen == null)
                    throw new ArgumentException($"El almacén con ID {linea.IdAlmacenDestino} no existe.");
            }

            var transaccionPropia = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var orden = new OrdenCompra
                {
                    Folio = await GenerarFolioAsync(),
                    IdProveedor = dto.IdProveedor,
                    FechaEmision = DateTime.Now,
                    FechaEntregaEstimada = dto.FechaEntregaEstimada,
                    IdUsuarioEmite = userId,
                    Estado = EstadoOrdenCompra.Emitida,
                    Moneda = dto.Moneda,
                    Observaciones = dto.Observaciones
                };

                foreach (var linea in dto.Lineas)
                {
                    orden.Detalles.Add(new OrdenCompraDetalle
                    {
                        IdMaterial = linea.IdMaterial,
                        IdAlmacenDestino = linea.IdAlmacenDestino,
                        CantidadPedida = linea.Cantidad,
                        CantidadRecibida = 0,
                        PrecioUnitarioPactado = linea.PrecioUnitario
                    });
                }

                await _unitOfWork.OrdenesCompra.AddAsync(orden);
                await _unitOfWork.CompleteAsync();

                // Amarrar cada renglón de la orden con las requisiciones que cubre.
                foreach (var detalleOrden in orden.Detalles)
                {
                    var porCubrir = detalleOrden.CantidadPedida;

                    // Del más antiguo al más nuevo: si lo pedido no alcanza para
                    // todos los que esperan, se cubre a quien lleva más tiempo.
                    var candidatos = (await _unitOfWork.RequisicionesEPPDetalle.FindAsync(
                            d => d.EstadoRenglon == EstadoRenglonRequisicion.PorComprar
                                 && d.IdMaterial == detalleOrden.IdMaterial,
                            includeProperties: "Requisicion"))
                        .Where(d => d.Requisicion != null
                                    && d.Requisicion.IdAlmacen == detalleOrden.IdAlmacenDestino)
                        .OrderBy(d => d.Requisicion!.FechaSolicitud)
                        .ThenBy(d => d.IdRequisicionDetalle)
                        .ToList();

                    foreach (var renglon in candidatos)
                    {
                        if (porCubrir <= 0) break;

                        var cubre = Math.Min(porCubrir, renglon.CantidadSolicitada);

                        await _unitOfWork.RequisicionesCobertura.AddAsync(new RequisicionCobertura
                        {
                            IdRequisicionDetalle = renglon.IdRequisicionDetalle,
                            Origen = OrigenCobertura.OrdenCompra,
                            IdOrdenCompraDetalle = detalleOrden.IdOrdenCompraDetalle,
                            Cantidad = cubre
                        });

                        // Sale de la bandeja: ya está pedido.
                        renglon.EstadoRenglon = EstadoRenglonRequisicion.EnOrdenCompra;
                        _unitOfWork.RequisicionesEPPDetalle.Update(renglon);

                        porCubrir -= cubre;
                    }
                }

                await _unitOfWork.CompleteAsync();
                if (transaccionPropia) await _unitOfWork.CommitTransactionAsync();

                return orden;
            }
            catch
            {
                if (transaccionPropia) await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> CancelarAsync(int id, string motivo, string userId)
        {
            var orden = await _unitOfWork.OrdenesCompra.GetByIdAsync(id, includeProperties: "Detalles");
            if (orden == null) return false;

            if (orden.Estado == EstadoOrdenCompra.Cancelada)
                throw new InvalidOperationException("La orden ya está cancelada.");

            if (orden.Detalles.Any(d => d.CantidadRecibida > 0))
                throw new InvalidOperationException(
                    "No se puede cancelar: ya se recibió material de esta orden.");

            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("Debe indicar el motivo de la cancelación.");

            var transaccionPropia = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var idsDetalle = orden.Detalles.Select(d => d.IdOrdenCompraDetalle).ToList();

                var coberturas = (await _unitOfWork.RequisicionesCobertura.FindAsync(
                        c => c.IdOrdenCompraDetalle != null
                             && idsDetalle.Contains(c.IdOrdenCompraDetalle.Value),
                        includeProperties: "RequisicionDetalle"))
                    .ToList();

                foreach (var cobertura in coberturas)
                {
                    // El renglón vuelve a la bandeja de faltantes.
                    if (cobertura.RequisicionDetalle != null &&
                        cobertura.RequisicionDetalle.EstadoRenglon == EstadoRenglonRequisicion.EnOrdenCompra)
                    {
                        cobertura.RequisicionDetalle.EstadoRenglon = EstadoRenglonRequisicion.PorComprar;
                        _unitOfWork.RequisicionesEPPDetalle.Update(cobertura.RequisicionDetalle);
                    }

                    _unitOfWork.RequisicionesCobertura.Remove(cobertura);
                }

                orden.Estado = EstadoOrdenCompra.Cancelada;
                orden.MotivoCancelacion = motivo;
                _unitOfWork.OrdenesCompra.Update(orden);

                var result = await _unitOfWork.CompleteAsync() > 0;
                if (transaccionPropia) await _unitOfWork.CommitTransactionAsync();

                return result;
            }
            catch
            {
                if (transaccionPropia) await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<string> GenerarFolioAsync()
        {
            var anio = DateTime.Now.Year;
            var prefijo = $"OC-{anio}-";

            var ordenes = await _unitOfWork.OrdenesCompra.FindAsync(
                o => o.Folio.StartsWith(prefijo));

            var ultimo = 0;
            if (ordenes.Any())
            {
                ultimo = ordenes
                    .Select(o => int.TryParse(o.Folio.Replace(prefijo, ""), out var n) ? n : 0)
                    .Max();
            }

            return $"{prefijo}{(ultimo + 1):D4}";
        }

        /// <summary>Último renglón de compra de un material, para sugerir precio y proveedor.</summary>
        private async Task<CompraEPPDetalle?> GetUltimaCompraAsync(int idMaterial)
        {
            var detalles = await _unitOfWork.ComprasEPPDetalle.FindAsync(
                d => d.IdMaterial == idMaterial,
                includeProperties: "Compra.Proveedor");

            return detalles
                .Where(d => d.Compra != null)
                .OrderByDescending(d => d.Compra!.FechaCompra)
                .ThenByDescending(d => d.IdCompraDetalle)
                .FirstOrDefault();
        }
    }
}
