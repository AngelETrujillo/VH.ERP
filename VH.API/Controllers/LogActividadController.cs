using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;
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

        /// <summary>
        /// Una página del log. Se deja el endpoint sin paginar para lo que aún lo
        /// consuma, pero las pantallas usan éste.
        /// </summary>
        [HttpGet("paginado")]
        public async Task<ActionResult<ResultadoPaginado<LogActividadResponseDto>>> GetPaginado(
            [FromQuery] ConsultaPaginada consulta,
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] string? userId)
        {
            return Ok(await _logService.GetPaginadoAsync(consulta, desde, hasta, userId));
        }

        [HttpGet("usuario/{userId}")]
        public async Task<ActionResult<IEnumerable<LogActividadResponseDto>>> GetByUsuario(string userId)
        {
            var logs = await _logService.GetByUsuarioAsync(userId);
            return Ok(logs);
        }
    }
}