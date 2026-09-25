using VH.Services.DTOs;
using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    /// <summary>
    /// El libro de movimientos del almacén.
    ///
    /// Todo lo que mueve existencia pasa por aquí: la entrada de una compra, la
    /// salida de una entrega, un ajuste por conteo, un traspaso entre obras. El
    /// servicio actualiza el saldo de <see cref="Inventario"/> y deja el renglón
    /// que lo explica, de modo que el saldo y su historia no puedan separarse.
    /// </summary>
    public interface IMovimientoInventarioService
    {
        /// <summary>
        /// Registra un movimiento y aplica su efecto sobre la existencia.
        ///
        /// No confirma los cambios: el llamador decide cuándo persistir, de forma
        /// que el movimiento y el documento que lo provoca caigan en la misma
        /// transacción.
        /// </summary>
        Task<MovimientoInventario> RegistrarAsync(
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
            string? observaciones = null);

        /// <summary>
        /// Una página del kardex con los totales del periodo.
        ///
        /// El saldo no hace falta arrastrarlo entre páginas: cada movimiento guarda
        /// el suyo desde que se registró, así que un renglón dice cuánto había
        /// después de él sin depender del anterior. Eso también permite leerlo del
        /// más reciente al más viejo sin que la columna pierda sentido.
        /// </summary>
        /// <param name="ascendente">
        /// Verdadero para leerlo como un kardex de papel, del movimiento más viejo
        /// al más nuevo. Por omisión va al revés, porque lo que se consulta en
        /// pantalla casi siempre es lo último que pasó.
        /// </param>
        Task<KardexDto> GetKardexPaginadoAsync(
            ConsultaPaginada consulta, int idMaterial, int idAlmacen,
            DateTime? desde = null, DateTime? hasta = null, bool ascendente = false);

        /// <summary>Kardex de un material en un almacén, con su saldo corrido.</summary>
        Task<IEnumerable<MovimientoInventario>> GetKardexAsync(
            int idMaterial, int idAlmacen, DateTime? desde = null, DateTime? hasta = null);

        /// <summary>
        /// Existencia que tenía un material en un almacén en una fecha dada,
        /// reconstruida sumando los movimientos hasta ese momento.
        /// </summary>
        Task<decimal> GetSaldoAFechaAsync(int idMaterial, int idAlmacen, DateTime fecha);

        /// <summary>
        /// Revisa los totales que el sistema guarda calculados —lo recibido de una
        /// orden, lo entregado de un renglón— contra la suma de sus partes, y las
        /// copias de lote, talla y caducidad contra su fuente.
        ///
        /// Son atajos deliberados para no sumar en cada consulta; esto comprueba que
        /// sigan diciendo la verdad.
        /// </summary>
        Task<IEnumerable<DesajusteDto>> RevisarDatosDuplicadosAsync();

        /// <summary>
        /// Compara el saldo guardado en Inventario contra la suma del kardex y
        /// devuelve las diferencias. Cero filas significa que la existencia está
        /// enteramente explicada por movimientos.
        /// </summary>
        Task<IEnumerable<DescuadreDto>> ReconciliarAsync();

        /// <summary>
        /// Corrige la existencia a lo que dice el conteo físico, dejando el
        /// movimiento de ajuste que explica la diferencia.
        /// </summary>
        Task<MovimientoInventario> AjustarAsync(
            int idMaterial, int idAlmacen, decimal existenciaContada, string motivo, string? userId);

        /// <summary>
        /// Mueve material de un almacén a otro: dos movimientos, salida y entrada,
        /// que se explican mutuamente.
        /// </summary>
        Task<(MovimientoInventario Salida, MovimientoInventario Entrada)> TraspasarAsync(
            int idMaterial, int idAlmacenOrigen, int idAlmacenDestino,
            decimal cantidad, string motivo, string? userId);

        /// <summary>Baja por daño, extravío o caducidad.</summary>
        Task<MovimientoInventario> RegistrarMermaAsync(
            int idMaterial, int idAlmacen, decimal cantidad, string motivo, string? userId);
    }
}
