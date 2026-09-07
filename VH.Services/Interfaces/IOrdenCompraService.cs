using VH.Services.DTOs;
using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    /// <summary>
    /// Consolidación de faltantes y órdenes de compra.
    ///
    /// Junta lo que piden las requisiciones autorizadas y que no puede surtirse
    /// del almacén, y lo convierte en un pedido al proveedor. Cada renglón de la
    /// orden queda amarrado a las requisiciones que viene a cubrir, de modo que al
    /// recibir se sepa a quién surtir y que nada se pida dos veces.
    /// </summary>
    public interface IOrdenCompraService
    {
        /// <summary>
        /// Lo que hay que comprar, agrupado por material y almacén destino.
        ///
        /// Sólo entran renglones autorizados sin cobertura. La disponibilidad se
        /// mira por almacén: un casco en la bodega de una obra no cubre a un
        /// obrero de otra que está a doscientos kilómetros.
        /// </summary>
        Task<IEnumerable<FaltanteDto>> GetFaltantesAsync(int? idAlmacen = null);

        Task<IEnumerable<OrdenCompra>> GetOrdenesAsync(EstadoOrdenCompra? estado = null);

        Task<OrdenCompra?> GetOrdenByIdAsync(int id);

        /// <summary>
        /// Emite la orden con las líneas elegidas y amarra cada una con los
        /// renglones de requisición que cubre, del más antiguo al más nuevo.
        /// Esos renglones salen de la bandeja de faltantes.
        ///
        /// Cuando la orden no alcanza para un renglón completo, ese renglón se
        /// parte: lo comprado sale de la bandeja y lo que falta se queda en ella.
        /// Devuelve los avisos de lo que quedó a medias o de sobra, para que el
        /// comprador vea que su orden se quedó corta antes de mandarla.
        /// </summary>
        Task<(OrdenCompra Orden, List<string> Avisos)> GenerarAsync(
            GenerarOrdenCompraRequestDto dto, string userId);

        /// <summary>
        /// Cancela la orden. Las coberturas se sueltan y sus renglones vuelven a
        /// la bandeja de faltantes. Sólo si no se ha recibido nada.
        /// </summary>
        Task<bool> CancelarAsync(int id, string motivo, string userId);

        /// <summary>Folio consecutivo del año: OC-2026-0001.</summary>
        Task<string> GenerarFolioAsync();
    }
}
