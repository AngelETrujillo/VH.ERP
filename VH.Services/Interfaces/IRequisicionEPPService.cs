using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    public interface IRequisicionEPPService
    {
        // Consultas
        Task<IEnumerable<RequisicionEPP>> GetAllAsync();
        Task<IEnumerable<RequisicionEPP>> GetByUsuarioAsync(string userId);
        Task<IEnumerable<RequisicionEPP>> GetByEmpleadoAsync(int idEmpleado);
        Task<IEnumerable<RequisicionEPP>> GetByEstadoAsync(EstadoRequisicion estado);
        Task<IEnumerable<RequisicionEPP>> GetPendientesAprobacionAsync();
        Task<IEnumerable<RequisicionEPP>> GetPendientesEntregaAsync();
        Task<RequisicionEPP?> GetByIdAsync(int id);

        // Operaciones
        Task<RequisicionEPP> CreateAsync(RequisicionEPP requisicion, string userId);
        Task<bool> AprobarAsync(int id, string userId, bool aprobada, string? motivoRechazo);
        Task<(bool Success, string? Error)> EntregarAsync(int id, string userId, string firmaDigital, string? fotoEvidencia, string? observaciones, List<(int IdDetalle, int IdCompra, decimal CantidadEntregada)> detalles);
        Task<bool> CancelarAsync(int id, string userId);

        // Utilidades
        Task<string> GenerarNumeroRequisicionAsync();
        Task<bool> PuedeVerRequisicionAsync(int idRequisicion, string userId);
    }
}