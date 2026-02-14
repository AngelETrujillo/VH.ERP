using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs.Analytics;
using VH.Services.Entities;
using VH.Services.Interfaces;
using System.Security.Claims;

namespace VH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AlertasConsumoController : ControllerBase
    {
        private readonly IAlertaConsumoService _alertaService;

        public AlertasConsumoController(IAlertaConsumoService alertaService)
        {
            _alertaService = alertaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAlertas([FromQuery] FiltroAlertasDto filtros)
        {
            var alertas = await _alertaService.GetAlertasAsync(filtros);
            return Ok(alertas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAlerta(int id)
        {
            var alerta = await _alertaService.GetAlertaByIdAsync(id);
            if (alerta == null) return NotFound();
            return Ok(alerta);
        }

        [HttpGet("resumen")]
        public async Task<IActionResult> GetResumen([FromQuery] int? idProyecto = null)
        {
            var resumen = await _alertaService.GetResumenAlertasAsync(idProyecto);
            return Ok(resumen);
        }

        [HttpGet("pendientes/empleado/{idEmpleado}")]
        public async Task<IActionResult> GetAlertasPendientesEmpleado(int idEmpleado)
        {
            var alertas = await _alertaService.GetAlertasPendientesEmpleadoAsync(idEmpleado);
            return Ok(alertas);
        }

        [HttpGet("proyecto/{idProyecto}")]
        public async Task<IActionResult> GetAlertasProyecto(int idProyecto, [FromQuery] bool soloPendientes = false)
        {
            var alertas = await _alertaService.GetAlertasProyectoAsync(idProyecto, soloPendientes);
            return Ok(alertas);
        }

        [HttpPut("{id}/revisar")]
        public async Task<IActionResult> RevisarAlerta(int id, [FromBody] RevisarAlertaRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var resultado = await _alertaService.RevisarAlertaAsync(id, userId, dto.NuevoEstado, dto.Observaciones);
            if (!resultado) return NotFound();

            return Ok(new { mensaje = "Alerta actualizada correctamente" });
        }

        [HttpPut("revisar-masivo")]
        public async Task<IActionResult> RevisarAlertasMasivo([FromBody] RevisarAlertasMasivoRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var cantidad = await _alertaService.RevisarAlertasMasivoAsync(dto.IdsAlertas, userId, dto.NuevoEstado, dto.Observaciones);
            return Ok(new { mensaje = $"{cantidad} alertas actualizadas correctamente" });
        }

        [HttpGet("configuracion")]
        public async Task<IActionResult> GetTodasConfiguraciones()
        {
            var configuraciones = await _alertaService.GetTodasConfiguracionesAsync();
            return Ok(configuraciones);
        }

        [HttpGet("configuracion/{idMaterial}")]
        public async Task<IActionResult> GetConfiguracionMaterial(int idMaterial)
        {
            var config = await _alertaService.GetConfiguracionMaterialAsync(idMaterial);
            if (config == null) return NotFound();
            return Ok(config);
        }

        [HttpPost("configuracion")]
        public async Task<IActionResult> GuardarConfiguracion([FromBody] ConfiguracionMaterialRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var config = await _alertaService.GuardarConfiguracionMaterialAsync(dto);
                return Ok(config);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }

    public record RevisarAlertasMasivoRequestDto(
        List<int> IdsAlertas,
        EstadoAlerta NuevoEstado,
        string? Observaciones
    );
}
