using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs.LogActividad;
using VH.Services.Interfaces;

namespace VH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,Administrador")]
    public class LogActividadController : ControllerBase
    {
        private readonly ILogActividadService _logService;

        public LogActividadController(ILogActividadService logService)
        {
            _logService = logService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LogActividadResponseDto>>> GetAll(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] string? userId)
        {
            var logs = await _logService.GetAllAsync(desde, hasta, userId);
            return Ok(logs);
        }

        [HttpGet("usuario/{userId}")]
        public async Task<ActionResult<IEnumerable<LogActividadResponseDto>>> GetByUsuario(string userId)
        {
            var logs = await _logService.GetByUsuarioAsync(userId);
            return Ok(logs);
        }
    }
}