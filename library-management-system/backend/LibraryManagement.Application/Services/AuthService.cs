using LibraryManagement.Application.DTOs;
using LibraryManagement.Application.Interfaces.Services;
using LibraryManagement.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagement.Application.Services;

public class AuthService : IAuthService
{
    private const string LoginProvider = "LibraryApp";
    private const string AccessTokenName = "AccessToken";
    private const string RefreshTokenName = "RefreshToken";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;

    public AuthService(UserManager<ApplicationUser> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userManager.FindByNameAsync(request.Username);
        if (existing != null) return null;

        var user = new ApplicationUser { UserName = request.Username };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) return null;

        // By default, new users are not admins. You can add logic here to assign admin role if needed.
        return await CreateAuthResponse(user);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return null;

        return await CreateAuthResponse(user);
    }

    public async Task<bool> LogoutAsync(string jwt)
    {
        var user = await FindUserByTokenAsync(AccessTokenName, jwt);
        if (user == null) return false;

        await _userManager.RemoveAuthenticationTokenAsync(user, LoginProvider, AccessTokenName);
        await _userManager.RemoveAuthenticationTokenAsync(user, LoginProvider, RefreshTokenName);
        return true;
    }

    public async Task<AuthResponse?> RefreshAsync(string refreshToken, int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return null;

        var stored = await _userManager.GetAuthenticationTokenAsync(user, LoginProvider, RefreshTokenName);
        if (stored == null || stored != refreshToken) return null;

        await _userManager.RemoveAuthenticationTokenAsync(user, LoginProvider, RefreshTokenName);
        await _userManager.RemoveAuthenticationTokenAsync(user, LoginProvider, AccessTokenName);

        return await CreateAuthResponse(user);
    }

    private async Task<AuthResponse> CreateAuthResponse(ApplicationUser user)
    {
        var jwt = GenerateToken(user);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        await _userManager.SetAuthenticationTokenAsync(user, LoginProvider, AccessTokenName, jwt);
        await _userManager.SetAuthenticationTokenAsync(user, LoginProvider, RefreshTokenName, refreshToken);

        return new AuthResponse(jwt, refreshToken, user.UserName!, user.Id, user.IsAdmin);
    }

    private async Task<ApplicationUser?> FindUserByTokenAsync(string tokenName, string tokenValue)
    {
        foreach (var user in _userManager.Users)
        {
            var token = await _userManager.GetAuthenticationTokenAsync(user, LoginProvider, tokenName);
            if (token == tokenValue) return user;
        }
        return null;
    }

    private string GenerateToken(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
