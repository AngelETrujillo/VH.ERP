using VH.Services.DTOs.Analytics;
using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    public interface IAlertaConsumoService
    {
        // ===== GENERACIÓN DE ALERTAS =====
        Task<List<AlertaConsumo>> EvaluarEntregaAsync(EntregaEPP entrega);
        Task<List<AlertaConsumo>> EvaluarRequisicionAsync(RequisicionEPP requisicion);
        Task<AlertaConsumo?> EvaluarSolicitudPrematuraAsync(int idEmpleado, int idMaterial, int? idEntrega = null, int? idRequisicion = null);
        Task<AlertaConsumo?> EvaluarExcesoCantidadAsync(int idEmpleado, int idMaterial, decimal cantidad, int? idEntrega = null);
        Task<AlertaConsumo?> EvaluarExcesoFrecuenciaAsync(int idEmpleado, int idMaterial);

        // ===== CONSULTAS =====
        Task<IEnumerable<AlertaConsumoResponseDto>> GetAlertasAsync(FiltroAlertasDto filtros);
        Task<AlertaConsumoResponseDto?> GetAlertaByIdAsync(int id);
        Task<IEnumerable<AlertaConsumoResponseDto>> GetAlertasPendientesEmpleadoAsync(int idEmpleado);
        Task<IEnumerable<AlertaConsumoResponseDto>> GetAlertasProyectoAsync(int idProyecto, bool soloPendientes = false);
        Task<ResumenAlertasDto> GetResumenAlertasAsync(int? idProyecto = null);

        // ===== GESTIÓN DE ESTADOS =====
        Task<bool> RevisarAlertaAsync(int idAlerta, string idUsuario, EstadoAlerta nuevoEstado, string? observaciones);
        Task<int> RevisarAlertasMasivoAsync(IEnumerable<int> idsAlertas, string idUsuario, EstadoAlerta nuevoEstado, string? observaciones);

        // ===== CONFIGURACIÓN =====
        Task<ConfiguracionMaterialEPP?> GetConfiguracionMaterialAsync(int idMaterial);
        Task<ConfiguracionMaterialEPP> GuardarConfiguracionMaterialAsync(ConfiguracionMaterialRequestDto dto);
        Task<IEnumerable<ConfiguracionMaterialResponseDto>> GetTodasConfiguracionesAsync();
    }
}