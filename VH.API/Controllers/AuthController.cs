using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VH.Services.DTOs.Auth;
using VH.Services.Interfaces;

namespace VH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogActividadService _logService;

        public AuthController(IAuthService authService, ILogActividadService logService)
        {
            _authService = authService;
            _logService = logService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.LoginAsync(request);

            if (result.Exitoso && result.Usuario != null)
            {
                await _logService.RegistrarAsync(result.Usuario.Id, "Login", "Usuario", null, "Inicio de sesión exitoso", ip);
            }

            if (!result.Exitoso)
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<LoginResponseDto>> RefreshToken([FromBody] RefreshTokenDto request)
        {
            var result = await _authService.RefreshTokenAsync(request);

            if (!result.Exitoso)
                return Unauthorized(result);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _authService.LogoutAsync(userId);
            await _logService.RegistrarAsync(userId, "Logout", "Usuario", null, "Cierre de sesión", ip);

            return Ok(new { mensaje = "Sesión cerrada exitosamente" });
        }

        [Authorize]
        [HttpPost("cambiar-password")]
        public async Task<IActionResult> CambiarPassword([FromBody] CambioPasswordDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var result = await _authService.CambiarPasswordAsync(userId, request);

            if (!result)
                return BadRequest(new { mensaje = "No se pudo cambiar la contraseña. Verifique la contraseña actual." });

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _logService.RegistrarAsync(userId, "CambioPassword", "Usuario", null, "Cambio de contraseña", ip);

            return Ok(new { mensaje = "Contraseña actualizada exitosamente" });
        }
    }
}