using VH.Services.DTOs.Rol;

namespace VH.Services.Interfaces
{
    public interface IRolService
    {
        Task<IEnumerable<RolResponseDto>> GetAllAsync();
        Task<RolResponseDto?> GetByIdAsync(string id);
        Task<(bool Exitoso, string Mensaje, RolResponseDto? Rol)> CreateAsync(RolRequestDto request);
        Task<(bool Exitoso, string Mensaje)> UpdateAsync(string id, RolRequestDto request);
        Task<(bool Exitoso, string Mensaje)> DeleteAsync(string id);
    }
}