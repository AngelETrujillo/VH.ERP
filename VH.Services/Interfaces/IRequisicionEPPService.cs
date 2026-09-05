using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    /// <summary>
    /// Requisiciones de material. El documento puede cubrir a varios empleados:
    /// cada renglón tiene su destino y su estado, y cada persona que recibe firma
    /// su propia entrega.
    /// </summary>
    public interface IRequisicionEPPService
    {
        // Consultas
        Task<IEnumerable<RequisicionEPP>> GetAllAsync();
        Task<IEnumerable<RequisicionEPP>> GetByUsuarioAsync(string userId);
        Task<IEnumerable<RequisicionEPP>> GetByEmpleadoAsync(int idEmpleado);
        Task<IEnumerable<RequisicionEPP>> GetByEstadoAsync(EstadoRequisicion estado);
        Task<IEnumerable<RequisicionEPP>> GetPendientesAprobacionAsync();

        /// <summary>Documentos con renglones autorizados que aún no se surten.</summary>
        Task<IEnumerable<RequisicionEPP>> GetPendientesEntregaAsync();

        Task<RequisicionEPP?> GetByIdAsync(int id);

        // Operaciones
        Task<RequisicionEPP> CreateAsync(RequisicionEPP requisicion, string userId);

        /// <summary>
        /// Autoriza o rechaza renglones. Con <paramref name="idsRenglones"/> nulo o
        /// vacío aplica a todos los que sigan solicitados, lo que permite autorizar
        /// el documento completo de un golpe o renglón por renglón.
        /// </summary>
        Task<bool> AprobarAsync(int id, string userId, bool aprobada, string? motivoRechazo,
            List<int>? idsRenglones = null);

        /// <summary>
        /// Entrega a UNA persona lo que le corresponde del documento y guarda su
        /// firma. El documento queda parcial mientras queden renglones de otras
        /// personas sin surtir.
        /// </summary>
        Task<(bool Success, string? Error)> EntregarAEmpleadoAsync(
            int id,
            int idEmpleado,
            string userId,
            string firmaDigital,
            string? fotoEvidencia,
            string? observaciones,
            List<(int IdDetalle, int IdCompraDetalle, decimal CantidadEntregada)> detalles);

        Task<bool> CancelarAsync(int id, string userId);

        // Utilidades
        Task<string> GenerarNumeroRequisicionAsync();
        Task<bool> PuedeVerRequisicionAsync(int idRequisicion, string userId);
    }
}
