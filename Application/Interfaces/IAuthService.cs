using Application.DTOs.Jwt;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponse?> LoginAsync(LoginDto loginDto);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
}