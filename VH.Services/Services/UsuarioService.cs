using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using VH.Services.DTOs.Usuario;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly UserManager<Usuario> _userManager;

        public UsuarioService(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IEnumerable<UsuarioResponseDto>> GetAllAsync()
        {
            var usuarios = _userManager.Users.ToList();
            var result = new List<UsuarioResponseDto>();

            foreach (var u in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new UsuarioResponseDto(
                    u.Id, u.Nombre, u.ApellidoPaterno, u.ApellidoMaterno,
                    u.NombreCompleto, u.Email!, u.UserName!,
                    u.Activo, u.FechaCreacion, u.UltimoAcceso, roles
                ));
            }
            return result;
        }

        public async Task<UsuarioResponseDto?> GetByIdAsync(string id)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) return null;

            var roles = await _userManager.GetRolesAsync(u);
            return new UsuarioResponseDto(
                u.Id, u.Nombre, u.ApellidoPaterno, u.ApellidoMaterno,
                u.NombreCompleto, u.Email!, u.UserName!,
                u.Activo, u.FechaCreacion, u.UltimoAcceso, roles
            );
        }

        public async Task<(bool Exitoso, string Mensaje, UsuarioResponseDto? Usuario)> CreateAsync(UsuarioRequestDto request)
        {
            var existeEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existeEmail != null)
                return (false, "Ya existe un usuario con ese email", null);

            var existeUserName = await _userManager.FindByNameAsync(request.UserName);
            if (existeUserName != null)
                return (false, "Ya existe un usuario con ese nombre de usuario", null);

            var usuario = new Usuario
            {
                Nombre = request.Nombre,
                ApellidoPaterno = request.ApellidoPaterno,
                ApellidoMaterno = request.ApellidoMaterno ?? string.Empty,
                Email = request.Email,
                UserName = request.UserName,
                Activo = request.Activo,
                FechaCreacion = DateTime.UtcNow
            };

            var password = string.IsNullOrWhiteSpace(request.Password) ? "Temporal123!" : request.Password;
            var result = await _userManager.CreateAsync(usuario, password);

            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);

            if (request.Roles != null && request.Roles.Any())
                await _userManager.AddToRolesAsync(usuario, request.Roles);

            var roles = await _userManager.GetRolesAsync(usuario);
            var response = new UsuarioResponseDto(
                usuario.Id, usuario.Nombre, usuario.ApellidoPaterno, usuario.ApellidoMaterno,
                usuario.NombreCompleto, usuario.Email, usuario.UserName,
                usuario.Activo, usuario.FechaCreacion, usuario.UltimoAcceso, roles
            );

            return (true, "Usuario creado exitosamente", response);
        }

        public async Task<(bool Exitoso, string Mensaje)> UpdateAsync(string id, UsuarioRequestDto request)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
                return (false, "Usuario no encontrado");

            var existeEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existeEmail != null && existeEmail.Id != id)
                return (false, "Ya existe otro usuario con ese email");

            var existeUserName = await _userManager.FindByNameAsync(request.UserName);
            if (existeUserName != null && existeUserName.Id != id)
                return (false, "Ya existe otro usuario con ese nombre de usuario");

            // Actualizar datos básicos
            usuario.Nombre = request.Nombre;
            usuario.ApellidoPaterno = request.ApellidoPaterno;
            usuario.ApellidoMaterno = request.ApellidoMaterno ?? string.Empty;
            usuario.Email = request.Email;
            usuario.UserName = request.UserName;
            usuario.Activo = request.Activo;

            var result = await _userManager.UpdateAsync(usuario);
            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

            // *** CAMBIO DE CONTRASEÑA ***
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                // Eliminar contraseña actual
                var removeResult = await _userManager.RemovePasswordAsync(usuario);
                if (!removeResult.Succeeded)
                    return (false, "Error al actualizar contraseña: " + string.Join(", ", removeResult.Errors.Select(e => e.Description)));

                // Agregar nueva contraseña
                var addResult = await _userManager.AddPasswordAsync(usuario, request.Password);
                if (!addResult.Succeeded)
                    return (false, "Error al establecer nueva contraseña: " + string.Join(", ", addResult.Errors.Select(e => e.Description)));
            }

            // Actualizar roles
            if (request.Roles != null)
            {
                var rolesActuales = await _userManager.GetRolesAsync(usuario);
                await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
                await _userManager.AddToRolesAsync(usuario, request.Roles);
            }

            return (true, "Usuario actualizado exitosamente");
        }
        public async Task<(bool Exitoso, string Mensaje)> DeleteAsync(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
                return (false, "Usuario no encontrado");

            var result = await _userManager.DeleteAsync(usuario);
            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

            return (true, "Usuario eliminado exitosamente");
        }

        public async Task<(bool Exitoso, string Mensaje)> ToggleActivoAsync(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
                return (false, "Usuario no encontrado");

            usuario.Activo = !usuario.Activo;
            var result = await _userManager.UpdateAsync(usuario);

            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

            return (true, usuario.Activo ? "Usuario activado" : "Usuario desactivado");
        }
    }
}