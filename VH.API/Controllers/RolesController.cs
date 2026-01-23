using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VH.Services.DTOs.Rol;
using VH.Services.Interfaces;

namespace VH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class RolesController : ControllerBase
    {
        private readonly IRolService _rolService;
        private readonly ILogActividadService _logService;

        public RolesController(IRolService rolService, ILogActividadService logService)
        {
            _rolService = rolService;
            _logService = logService;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Administrador")]
        public async Task<ActionResult<IEnumerable<RolResponseDto>>> GetAll()
        {
            var roles = await _rolService.GetAllAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RolResponseDto>> GetById(string id)
        {
            var rol = await _rolService.GetByIdAsync(id);
            if (rol == null) return NotFound();
            return Ok(rol);
        }

        [HttpPost]
        public async Task<ActionResult<RolResponseDto>> Create([FromBody] RolRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (exitoso, mensaje, rol) = await _rolService.CreateAsync(request);

            if (!exitoso)
                return BadRequest(new { mensaje });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _logService.RegistrarAsync(userId!, "Crear", "Rol", null, $"Rol creado: {request.Name}", ip);

            return CreatedAtAction(nameof(GetById), new { id = rol!.Id }, rol);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] RolRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (exitoso, mensaje) = await _rolService.UpdateAsync(id, request);

            if (!exitoso)
                return BadRequest(new { mensaje });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _logService.RegistrarAsync(userId!, "Actualizar", "Rol", null, $"Rol actualizado: {request.Name}", ip);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var (exitoso, mensaje) = await _rolService.DeleteAsync(id);

            if (!exitoso)
                return BadRequest(new { mensaje });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _logService.RegistrarAsync(userId!, "Eliminar", "Rol", null, $"Rol eliminado: {id}", ip);

            return NoContent();
        }
    }
}