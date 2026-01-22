using VH.Services.DTOs.Auth;

namespace VH.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenDto request);
        Task<bool> LogoutAsync(string userId);
        Task<bool> CambiarPasswordAsync(string userId, CambioPasswordDto request);
    }
}