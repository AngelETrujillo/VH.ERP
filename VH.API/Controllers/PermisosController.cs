using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VH.Services.DTOs.Modulo;
using VH.Services.DTOs.Permiso;
using VH.Services.Interfaces;

namespace VH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermisosController : ControllerBase
    {
        private readonly IPermisoService _permisoService;
        private readonly ILogActividadService _logService;

        public PermisosController(IPermisoService permisoService, ILogActividadService logService)
        {
            _permisoService = permisoService;
            _logService = logService;
        }

        [HttpGet("modulos")]
        [Authorize(Roles = "SuperAdmin,Administrador")]
        public async Task<ActionResult<IEnumerable<ModuloResponseDto>>> GetModulos()
        {
            var modulos = await _permisoService.GetAllModulosAsync();
            return Ok(modulos);
        }

        [HttpGet("rol/{idRol}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<PermisosRolResponseDto>> GetPermisosByRol(string idRol)
        {
            var permisos = await _permisoService.GetPermisosByRolAsync(idRol);
            if (permisos == null) return NotFound();
            return Ok(permisos);
        }

        [HttpPost("rol/{idRol}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> AsignarPermisos(string idRol, [FromBody] List<AsignarPermisoRequestDto> permisos)
        {
            var result = await _permisoService.AsignarPermisosAsync(idRol, permisos);
            if (!result) return BadRequest(new { mensaje = "Error al asignar permisos" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _logService.RegistrarAsync(userId!, "AsignarPermisos", "Rol", null, $"Permisos asignados al rol: {idRol}", ip);

            return Ok(new { mensaje = "Permisos asignados exitosamente" });
        }

        [HttpGet("usuario/{userId}")]
        [Authorize]
        public async Task<ActionResult<List<PermisoModuloDto>>> GetPermisosUsuario(string userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != userId && !User.IsInRole("SuperAdmin") && !User.IsInRole("Administrador"))
                return Forbid();

            var permisos = await _permisoService.GetPermisosUsuarioAsync(userId);
            return Ok(permisos);
        }

        [HttpGet("mis-permisos")]
        [Authorize]
        public async Task<ActionResult<List<PermisoModuloDto>>> GetMisPermisos()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var permisos = await _permisoService.GetPermisosUsuarioAsync(userId);
            return Ok(permisos);
        }
    }
}