using Microsoft.EntityFrameworkCore;
using VH.Services.DTOs;
using VH.Services.DTOs.LogActividad;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    public class LogActividadService : ILogActividadService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LogActividadService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task RegistrarAsync(string userId, string accion, string? entidad = null, int? idEntidad = null, string? descripcion = null, string? ip = null)
        {
            var log = new LogActividad
            {
                IdUsuario = userId,
                Accion = accion,
                Entidad = entidad,
                IdEntidad = idEntidad,
                Descripcion = descripcion,
                DireccionIP = ip,
                Fecha = DateTime.UtcNow
            };

            await _unitOfWork.LogsActividad.AddAsync(log);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<LogActividadResponseDto>> GetAllAsync(DateTime? desde = null, DateTime? hasta = null, string? userId = null)
        {
            var logs = await _unitOfWork.LogsActividad.FindAsync(
                filter: l =>
                    (desde == null || l.Fecha >= desde) &&
                    (hasta == null || l.Fecha <= hasta) &&
                    (userId == null || l.IdUsuario == userId),
                includeProperties: "Usuario"
            );

            return logs.OrderByDescending(l => l.Fecha).Select(l => new LogActividadResponseDto(
                l.IdLog,
                l.IdUsuario,
                l.Usuario?.NombreCompleto ?? "Desconocido",
                l.Accion,
                l.Entidad,
                l.IdEntidad,
                l.Descripcion,
                l.DireccionIP,
                l.Fecha
            ));
        }

        public async Task<ResultadoPaginado<LogActividadResponseDto>> GetPaginadoAsync(
            ConsultaPaginada consulta, DateTime? desde = null, DateTime? hasta = null, string? userId = null)
        {
            var texto = consulta.TextoLimpio;

            var pagina = await _unitOfWork.LogsActividad.GetPaginadoAsync(
                consulta,
                filtro: l =>
                    (desde == null || l.Fecha >= desde) &&
                    (hasta == null || l.Fecha <= hasta) &&
                    (userId == null || l.IdUsuario == userId) &&
                    (texto == null ||
                     l.Accion.Contains(texto) ||
                     (l.Entidad != null && l.Entidad.Contains(texto)) ||
                     (l.Descripcion != null && l.Descripcion.Contains(texto)) ||
                     // Por las tres columnas y no por NombreCompleto: esa es una
                     // propiedad calculada y EF no puede traducirla a SQL.
                     (l.Usuario != null && (l.Usuario.Nombre.Contains(texto) ||
                                            l.Usuario.ApellidoPaterno.Contains(texto) ||
                                            l.Usuario.ApellidoMaterno.Contains(texto)))),
                orden: q => q.OrderByDescending(l => l.Fecha),
                includeProperties: "Usuario");

            return pagina.Convertir(l => new LogActividadResponseDto(
                l.IdLog,
                l.IdUsuario,
                l.Usuario?.NombreCompleto ?? "Desconocido",
                l.Accion,
                l.Entidad,
                l.IdEntidad,
                l.Descripcion,
                l.DireccionIP,
                l.Fecha));
        }

        public async Task<IEnumerable<LogActividadResponseDto>> GetByUsuarioAsync(string userId)
        {
            return await GetAllAsync(userId: userId);
        }
    }
}