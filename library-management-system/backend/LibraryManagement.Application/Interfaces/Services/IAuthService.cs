using LibraryManagement.Application.DTOs;

namespace LibraryManagement.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RefreshAsync(string refreshToken, int userId);
    Task<bool> LogoutAsync(string token);
}
