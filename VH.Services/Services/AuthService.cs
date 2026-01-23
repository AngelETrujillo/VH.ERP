using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VH.Services.DTOs.Auth;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var usuario = await _userManager.FindByNameAsync(request.Usuario)
                          ?? await _userManager.FindByEmailAsync(request.Usuario);

            if (usuario == null)
                return new LoginResponseDto(false, null, null, null, "Usuario no encontrado", null);

            if (!usuario.Activo)
                return new LoginResponseDto(false, null, null, null, "Usuario desactivado", null);

            var result = await _signInManager.CheckPasswordSignInAsync(usuario, request.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
                return new LoginResponseDto(false, null, null, null, "Cuenta bloqueada por múltiples intentos fallidos", null);

            if (!result.Succeeded)
                return new LoginResponseDto(false, null, null, null, "Contraseña incorrecta", null);

            var roles = await _userManager.GetRolesAsync(usuario);
            var token = GenerarToken(usuario, roles);
            var refreshToken = GenerarRefreshToken();
            var expiracion = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpirationMinutes"] ?? "60"));

            usuario.RefreshToken = refreshToken;
            usuario.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            usuario.UltimoAcceso = DateTime.UtcNow;
            await _userManager.UpdateAsync(usuario);

            var usuarioDto = new UsuarioLogueadoDto(usuario.Id, usuario.NombreCompleto, usuario.Email!, roles);

            return new LoginResponseDto(true, token, refreshToken, expiracion, "Login exitoso", usuarioDto);
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenDto request)
        {
            var principal = GetPrincipalFromExpiredToken(request.Token);
            if (principal == null)
                return new LoginResponseDto(false, null, null, null, "Token inválido", null);

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return new LoginResponseDto(false, null, null, null, "Token inválido", null);

            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null || usuario.RefreshToken != request.RefreshToken || usuario.RefreshTokenExpiry <= DateTime.UtcNow)
                return new LoginResponseDto(false, null, null, null, "Refresh token inválido o expirado", null);

            var roles = await _userManager.GetRolesAsync(usuario);
            var newToken = GenerarToken(usuario, roles);
            var newRefreshToken = GenerarRefreshToken();
            var expiracion = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpirationMinutes"] ?? "60"));

            usuario.RefreshToken = newRefreshToken;
            usuario.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(usuario);

            var usuarioDto = new UsuarioLogueadoDto(usuario.Id, usuario.NombreCompleto, usuario.Email!, roles);

            return new LoginResponseDto(true, newToken, newRefreshToken, expiracion, "Token renovado", usuarioDto);
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null) return false;

            usuario.RefreshToken = null;
            usuario.RefreshTokenExpiry = null;
            await _userManager.UpdateAsync(usuario);

            return true;
        }

        public async Task<bool> CambiarPasswordAsync(string userId, CambioPasswordDto request)
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null) return false;

            var result = await _userManager.ChangePasswordAsync(usuario, request.PasswordActual, request.PasswordNuevo);
            return result.Succeeded;
        }

        private string GenerarToken(Usuario usuario, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id),
                new(ClaimTypes.Name, usuario.UserName!),
                new(ClaimTypes.Email, usuario.Email!),
                new("NombreCompleto", usuario.NombreCompleto)
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpirationMinutes"] ?? "60"));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerarRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
                ValidateLifetime = false
            };

            try
            {
                var principal = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out var securityToken);
                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    return null;

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}