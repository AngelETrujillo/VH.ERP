using Microsoft.AspNetCore.Identity;
using VH.Services.DTOs;
using VH.Services.DTOs.Rol;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    public class RolService : IRolService
    {
        private readonly RoleManager<Rol> _roleManager;

        public RolService(RoleManager<Rol> roleManager)
        {
            _roleManager = roleManager;
        }

        public Task<ResultadoPaginado<RolResponseDto>> GetPaginadoAsync(ConsultaPaginada consulta)
        {
            var texto = consulta.TextoLimpio;
            var query = _roleManager.Roles.AsQueryable();

            if (texto != null)
                query = query.Where(r =>
                    (r.Name != null && r.Name.Contains(texto)) ||
                    (r.Descripcion != null && r.Descripcion.Contains(texto)));

            var total = query.Count();

            var renglones = query
                .OrderBy(r => r.Name)
                .Skip(consulta.Salto).Take(consulta.Tamano)
                .ToList()
                .Select(r => new RolResponseDto(r.Id, r.Name!, r.Descripcion, r.Activo, r.FechaCreacion))
                .ToList();

            return Task.FromResult(new ResultadoPaginado<RolResponseDto>
            {
                Renglones = renglones,
                Total = total,
                Pagina = consulta.Pagina,
                Tamano = consulta.Tamano,
                Buscado = texto
            });
        }

        public async Task<IEnumerable<RolResponseDto>> GetAllAsync()
        {
            var roles = _roleManager.Roles.ToList();
            return roles.Select(r => new RolResponseDto(
                r.Id,
                r.Name!,
                r.Descripcion,
                r.Activo,
                r.FechaCreacion
            ));
        }

        public async Task<RolResponseDto?> GetByIdAsync(string id)
        {
            var rol = await _roleManager.FindByIdAsync(id);
            if (rol == null) return null;

            return new RolResponseDto(
                rol.Id,
                rol.Name!,
                rol.Descripcion,
                rol.Activo,
                rol.FechaCreacion
            );
        }

        public async Task<(bool Exitoso, string Mensaje, RolResponseDto? Rol)> CreateAsync(RolRequestDto request)
        {
            var existe = await _roleManager.RoleExistsAsync(request.Name);
            if (existe)
                return (false, "Ya existe un rol con ese nombre", null);

            var rol = new Rol
            {
                Name = request.Name,
                Descripcion = request.Descripcion,
                Activo = request.Activo,
                FechaCreacion = DateTime.UtcNow
            };

            var result = await _roleManager.CreateAsync(rol);
            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);

            var response = new RolResponseDto(rol.Id, rol.Name, rol.Descripcion, rol.Activo, rol.FechaCreacion);
            return (true, "Rol creado exitosamente", response);
        }

        public async Task<(bool Exitoso, string Mensaje)> UpdateAsync(string id, RolRequestDto request)
        {
            var rol = await _roleManager.FindByIdAsync(id);
            if (rol == null)
                return (false, "Rol no encontrado");

            var existeOtro = _roleManager.Roles.Any(r => r.Name == request.Name && r.Id != id);
            if (existeOtro)
                return (false, "Ya existe otro rol con ese nombre");

            rol.Name = request.Name;
            rol.Descripcion = request.Descripcion;
            rol.Activo = request.Activo;

            var result = await _roleManager.UpdateAsync(rol);
            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

            return (true, "Rol actualizado exitosamente");
        }

        public async Task<(bool Exitoso, string Mensaje)> DeleteAsync(string id)
        {
            var rol = await _roleManager.FindByIdAsync(id);
            if (rol == null)
                return (false, "Rol no encontrado");

            var result = await _roleManager.DeleteAsync(rol);
            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

            return (true, "Rol eliminado exitosamente");
        }
    }
}