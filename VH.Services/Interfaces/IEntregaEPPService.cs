using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    /// <summary>
    /// Servicio para gestionar entregas de EPP a empleados.
    /// Maneja el descuento automático del inventario.
    /// </summary>
    public interface IEntregaEPPService
    {
        /// <summary>
        /// Obtiene todas las entregas, opcionalmente filtradas por empleado
        /// </summary>
        Task<IEnumerable<EntregaEPP>> GetEntregasAsync(int? idEmpleado = null);

        /// <summary>
        /// Obtiene una entrega por su ID
        /// </summary>
        Task<EntregaEPP?> GetEntregaByIdAsync(int id);

        /// <summary>
        /// Obtiene las entregas de un material específico
        /// </summary>
        Task<IEnumerable<EntregaEPP>> GetEntregasByMaterialAsync(int idMaterial);

        /// <summary>
        /// Registra una nueva entrega de EPP.
        /// - Valida que el lote tenga suficiente cantidad disponible
        /// - Descuenta del lote (CantidadDisponible)
        /// - Actualiza el inventario (resta existencia)
        /// - Retorna alerta si se llega al stock mínimo
        /// </summary>
        Task<(EntregaEPP Entrega, string? Alerta)> CreateEntregaAsync(EntregaEPP entrega);

        /// <summary>
        /// Actualiza una entrega existente.
        /// Si cambia la cantidad, ajusta el inventario correspondiente.
        /// </summary>
        Task<(bool Success, string? Alerta)> UpdateEntregaAsync(EntregaEPP entrega);

        /// <summary>
        /// Elimina una entrega y devuelve la cantidad al inventario
        /// </summary>
        Task<bool> DeleteEntregaAsync(int id);
    }
}