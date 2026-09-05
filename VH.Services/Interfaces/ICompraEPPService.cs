using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    /// <summary>
    /// Compras de material. Una compra es el documento del proveedor y cada uno de
    /// sus renglones es un lote de inventario, del que después salen las entregas.
    /// </summary>
    public interface ICompraEPPService
    {
        /// <summary>
        /// Compras filtradas. Los filtros de material y almacén miran los renglones:
        /// devuelven la compra si alguno de ellos coincide.
        /// </summary>
        Task<IEnumerable<CompraEPP>> GetComprasAsync(
            int? idMaterial = null,
            int? idProveedor = null,
            int? idAlmacen = null);

        Task<CompraEPP?> GetCompraByIdAsync(int id);

        /// <summary>Un renglón concreto, que es un lote.</summary>
        Task<CompraEPPDetalle?> GetLoteByIdAsync(int idCompraDetalle);

        /// <summary>
        /// Lotes con existencia de un material en un almacén, para elegir de cuál
        /// surtir. Ordenados por caducidad y luego por antigüedad, de modo que lo
        /// primero de la lista sea lo primero que conviene sacar.
        /// </summary>
        Task<IEnumerable<CompraEPPDetalle>> GetLotesDisponiblesAsync(int idMaterial, int idAlmacen);

        /// <summary>
        /// Registra la compra completa con todos sus renglones, en una transacción:
        /// crea los lotes, suma las existencias y actualiza el costo de referencia
        /// de cada material. Devuelve las alertas de stock que se hayan disparado.
        /// </summary>
        Task<(CompraEPP Compra, List<string> Alertas)> CreateCompraAsync(CompraEPP compra);

        /// <summary>
        /// Actualiza los datos del documento (fecha, folio, CFDI, IVA, notas).
        /// Ni los renglones ni sus precios se tocan aquí: cambiar el precio de un
        /// lote ya consumido reescribiría el costo de entregas pasadas.
        /// </summary>
        Task<bool> UpdateCompraAsync(int idCompra, DateTime fechaCompra, string? numeroDocumento,
            string? uuidCFDI, decimal iva, string? observaciones);

        /// <summary>
        /// Cancela la compra completa con sus renglones y revierte las existencias.
        /// Sólo si ningún renglón se ha consumido.
        /// </summary>
        Task<bool> DeleteCompraAsync(int id);

        /// <summary>Precios pagados por un material, para comparar y negociar.</summary>
        Task<IEnumerable<CompraEPPDetalle>> GetHistorialPreciosAsync(int idMaterial, int? idProveedor = null);
    }
}
