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

        // ===== RESERVA DE EXISTENCIA =====

        /// <summary>
        /// Lo que todavía puede prometerse de un material en un almacén:
        /// existencia menos lo ya apartado. Cero si no hay registro de inventario.
        /// </summary>
        Task<decimal> GetDisponibleAsync(int idMaterial, int idAlmacen);

        /// <summary>
        /// Aparta cantidad para un renglón autorizado. Devuelve false si el
        /// disponible no alcanza, sin tocar nada: el renglón queda por comprar.
        /// No confirma los cambios; el llamador decide cuándo persistir.
        /// </summary>
        Task<bool> ReservarAsync(int idMaterial, int idAlmacen, decimal cantidad);

        /// <summary>
        /// Suelta una reserva sin mover la existencia: el renglón se canceló o se
        /// rechazó y el material vuelve a estar disponible para otros.
        /// </summary>
        Task LiberarReservaAsync(int idMaterial, int idAlmacen, decimal cantidad);

        /// <summary>
        /// Consume una reserva al surtirla. Sólo baja el comprometido: la salida
        /// de la existencia la hace el servicio de entregas.
        /// </summary>
        Task ConsumirReservaAsync(int idMaterial, int idAlmacen, decimal cantidad);
    }
}