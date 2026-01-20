using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    /// <summary>
    /// Servicio para gestionar el inventario de materiales EPP.
    /// El inventario se actualiza automáticamente con compras y entregas.
    /// </summary>
    public interface IInventarioService
    {
        /// <summary>
        /// Obtiene todos los registros de inventario
        /// </summary>
        Task<IEnumerable<Inventario>> GetAllInventariosAsync();

        /// <summary>
        /// Obtiene los inventarios de un almacén específico
        /// </summary>
        Task<IEnumerable<Inventario>> GetInventariosByAlmacenAsync(int idAlmacen);

        /// <summary>
        /// Obtiene los inventarios de un material en todos los almacenes
        /// </summary>
        Task<IEnumerable<Inventario>> GetInventariosByMaterialAsync(int idMaterial);

        /// <summary>
        /// Obtiene un registro de inventario por su ID
        /// </summary>
        Task<Inventario?> GetInventarioByIdAsync(int id);

        /// <summary>
        /// Obtiene el inventario de un material en un almacén específico
        /// </summary>
        Task<Inventario?> GetInventarioByMaterialAlmacenAsync(int idMaterial, int idAlmacen);

        /// <summary>
        /// Crea o actualiza la configuración de inventario (stock mínimo, máximo, ubicación).
        /// La existencia se calcula automáticamente.
        /// </summary>
        Task<Inventario> CreateOrUpdateInventarioAsync(Inventario inventario);

        /// <summary>
        /// Actualiza solo la configuración de inventario (no la existencia)
        /// </summary>
        Task<bool> UpdateConfiguracionAsync(int id, decimal stockMinimo, decimal stockMaximo, string ubicacion);

        /// <summary>
        /// Elimina un registro de inventario (solo si no hay compras asociadas)
        /// </summary>
        Task<bool> DeleteInventarioAsync(int id);

        /// <summary>
        /// Obtiene el stock global de un material (suma de todos los almacenes)
        /// </summary>
        Task<decimal> GetStockGlobalMaterialAsync(int idMaterial);

        /// <summary>
        /// Obtiene todos los inventarios con alertas (bajo stock o sobre stock)
        /// </summary>
        Task<IEnumerable<Inventario>> GetInventariosConAlertasAsync();

        /// <summary>
        /// Recalcula la existencia de un inventario basándose en compras y entregas.
        /// Usado para sincronización o corrección de datos.
        /// </summary>
        Task<decimal> RecalcularExistenciaAsync(int idMaterial, int idAlmacen);
    }
}