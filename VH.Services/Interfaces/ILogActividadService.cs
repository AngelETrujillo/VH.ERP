using VH.Services.DTOs.LogActividad;

namespace VH.Services.Interfaces
{
    public interface ILogActividadService
    {
        Task RegistrarAsync(string userId, string accion, string? entidad = null, int? idEntidad = null, string? descripcion = null, string? ip = null);
        Task<IEnumerable<LogActividadResponseDto>> GetAllAsync(DateTime? desde = null, DateTime? hasta = null, string? userId = null);
        Task<IEnumerable<LogActividadResponseDto>> GetByUsuarioAsync(string userId);
    }
}