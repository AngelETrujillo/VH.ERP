using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    /// <summary>
    /// Servicio para gestionar compras/lotes de materiales EPP.
    /// Maneja el registro de compras y actualización automática de inventario.
    /// </summary>
    public interface ICompraEPPService
    {
        /// <summary>
        /// Obtiene todas las compras, opcionalmente filtradas
        /// </summary>
        Task<IEnumerable<CompraEPP>> GetComprasAsync(
            int? idMaterial = null,
            int? idProveedor = null,
            int? idAlmacen = null);

        /// <summary>
        /// Obtiene una compra por su ID
        /// </summary>
        Task<CompraEPP?> GetCompraByIdAsync(int id);

        /// <summary>
        /// Obtiene los lotes con disponibilidad para un material en un almacén específico.
        /// Usado para seleccionar de qué lote hacer entregas.
        /// </summary>
        Task<IEnumerable<CompraEPP>> GetLotesDisponiblesAsync(int idMaterial, int idAlmacen);

        /// <summary>
        /// Registra una nueva compra de material.
        /// - Crea el registro de compra
        /// - Actualiza el inventario (suma existencia)
        /// - Actualiza el precio del material (último precio)
        /// - Retorna alerta si se excede el stock máximo
        /// </summary>
        Task<(CompraEPP Compra, string? Alerta)> CreateCompraAsync(CompraEPP compra);

        /// <summary>
        /// Actualiza una compra existente (solo datos básicos, no cantidades)
        /// </summary>
        Task<bool> UpdateCompraAsync(CompraEPP compra);

        /// <summary>
        /// Elimina una compra (solo si no tiene entregas asociadas y tiene toda la cantidad disponible)
        /// </summary>
        Task<bool> DeleteCompraAsync(int id);

        /// <summary>
        /// Obtiene el historial de precios de un material por proveedor
        /// Para análisis y negociación
        /// </summary>
        Task<IEnumerable<CompraEPP>> GetHistorialPreciosAsync(int idMaterial, int? idProveedor = null);
    }
}