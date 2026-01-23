using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VH.Services.DTOs.Usuario;
using VH.Services.Interfaces;

namespace VH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,Administrador")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogActividadService _logService;

        public UsuariosController(IUsuarioService usuarioService, ILogActividadService logService)
        {
            _usuarioService = usuarioService;
            _logService = logService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioResponseDto>>> GetAll()
        {
            var usuarios = await _usuarioService.GetAllAsync();
            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioResponseDto>> GetById(string id)
        {
            var usuario = await _usuarioService.GetByIdAsync(id);
            if (usuario == null) return NotFound();
            return Ok(usuario);
        }

        [HttpPost]
        public async Task<ActionResult<UsuarioResponseDto>> Create([FromBody] UsuarioRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (exitoso, mensaje, usuario) = await _usuarioService.CreateAsync(request);

            if (!exitoso)
                return BadRequest(new { mensaje });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _logService.RegistrarAsync(userId!, "Crear", "Usuario", null, $"Usuario creado: {request.UserName}", ip);

            return CreatedAtAction(nameof(GetById), new { id = usuario!.Id }, usuario);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UsuarioRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (exitoso, mensaje) = await _usuarioService.UpdateAsync(id, request);

            if (!exitoso)
                return BadRequest(new { mensaje });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _logService.RegistrarAsync(userId!, "Actualizar", "Usuario", null, $"Usuario actualizado: {request.UserName}", ip);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(string id)
        {
            var (exitoso, mensaje) = await _usuarioService.DeleteAsync(id);

            if (!exitoso)
                return BadRequest(new { mensaje });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _logService.RegistrarAsync(userId!, "Eliminar", "Usuario", null, $"Usuario eliminado: {id}", ip);

            return NoContent();
        }

        [HttpPatch("{id}/toggle-activo")]
        public async Task<IActionResult> ToggleActivo(string id)
        {
            var (exitoso, mensaje) = await _usuarioService.ToggleActivoAsync(id);

            if (!exitoso)
                return BadRequest(new { mensaje });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _logService.RegistrarAsync(userId!, "ToggleActivo", "Usuario", null, mensaje, ip);

            return Ok(new { mensaje });
        }
    }
}