using VH.Services.DTOs;
using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    /// <summary>
    /// La entrada del material al almacén contra una orden de compra.
    ///
    /// Recibir es lo que convierte un pedido en existencia: genera el lote con el
    /// precio realmente pagado, lo mete al kardex, avanza la orden y avisa a quién
    /// le toca lo que acaba de llegar.
    /// </summary>
    public interface IRecepcionCompraService
    {
        /// <summary>
        /// La bandeja del almacenista: órdenes con material en camino, una entrada
        /// por cada almacén al que deben llegar. Lo más retrasado, primero.
        /// </summary>
        Task<IEnumerable<OrdenPorRecibirDto>> GetPorRecibirAsync(int? idAlmacen = null);

        /// <summary>
        /// Lo que hay que contar: los renglones de esa orden que van a ese almacén,
        /// con lo que falta por llegar y quién lo está esperando.
        /// </summary>
        Task<PreparacionRecepcionDto?> GetPreparacionAsync(int idOrdenCompra, int idAlmacen);

        Task<IEnumerable<RecepcionCompra>> GetRecepcionesAsync(int? idOrdenCompra = null, int? idAlmacen = null);

        Task<RecepcionCompra?> GetRecepcionByIdAsync(int id);

        /// <summary>
        /// Registra la llegada. En una sola transacción: crea la recepción, genera
        /// el lote con lo aceptado, lo asienta en el kardex, suma lo recibido a los
        /// renglones de la orden y pasa a <see cref="EstadoRenglonRequisicion.Recibido"/>
        /// los renglones de requisición que este material alcanza a cubrir,
        /// apartándoles la cantidad.
        ///
        /// Devuelve los avisos que el almacenista debe ver: diferencias de precio
        /// contra lo pactado, material rechazado y a quién ya se le puede surtir.
        /// </summary>
        Task<(RecepcionCompra Recepcion, List<string> Avisos)> RecibirAsync(
            RecibirOrdenRequestDto dto, string userId);

        /// <summary>Folio consecutivo del año: REC-2026-0001.</summary>
        Task<string> GenerarFolioAsync();
    }
}
