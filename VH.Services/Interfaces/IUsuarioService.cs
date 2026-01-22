using VH.Services.DTOs.Usuario;

namespace VH.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<IEnumerable<UsuarioResponseDto>> GetAllAsync();
        Task<UsuarioResponseDto?> GetByIdAsync(string id);
        Task<(bool Exitoso, string Mensaje, UsuarioResponseDto? Usuario)> CreateAsync(UsuarioRequestDto request);
        Task<(bool Exitoso, string Mensaje)> UpdateAsync(string id, UsuarioRequestDto request);
        Task<(bool Exitoso, string Mensaje)> DeleteAsync(string id);
        Task<(bool Exitoso, string Mensaje)> ToggleActivoAsync(string id);
    }
}