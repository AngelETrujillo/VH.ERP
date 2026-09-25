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

        /// <summary>Una página del historial de recepciones.</summary>
        Task<ResultadoPaginado<RecepcionCompra>> GetHistorialPaginadoAsync(
            ConsultaPaginada consulta, int? idOrdenCompra = null, int? idAlmacen = null);

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

        /// <summary>
        /// Deshace una recepción que se capturó mal.
        ///
        /// Recibir de más, de menos o contra la orden equivocada es un error de
        /// todos los días, y hasta ahora no había forma de corregirlo desde el
        /// sistema. Deshacerla devuelve la existencia, suelta lo que se había
        /// apartado, regresa la orden a lo que estaba y deja en el kardex el
        /// movimiento que lo explica: el rastro no se borra, se contrapesa.
        ///
        /// Sólo se puede deshacer la última recepción de esa orden en ese almacén,
        /// y sólo si nada de lo que trajo se ha entregado todavía.
        /// </summary>
        Task<(bool Exito, string? Error)> CancelarAsync(int idRecepcion, string motivo, string userId);

        /// <summary>
        /// Dónde terminó cada pieza de un lote de fábrica: lo que queda en el
        /// anaquel y quién se llevó el resto.
        ///
        /// Es lo que hace útil capturar el lote del proveedor. Sin esto el dato se
        /// guarda y no sirve para nada: cuando avisan de un defecto, la única
        /// salida es recoger todo y revisar a ojo.
        /// </summary>
        Task<IEnumerable<RastreoLoteDto>> RastrearLoteAsync(string loteProveedor);

        /// <summary>Folio consecutivo del año: REC-2026-0001.</summary>
        Task<string> GenerarFolioAsync();
    }
}
