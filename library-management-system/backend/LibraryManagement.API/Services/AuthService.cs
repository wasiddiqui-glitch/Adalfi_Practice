using LibraryManagement.API.Data;
using LibraryManagement.API.DTOs;
using LibraryManagement.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagement.API.Services;

public class AuthService : IAuthService
{
    private const string LoginProvider = "LibraryApp";
    private const string AccessTokenName = "AccessToken";
    private const string RefreshTokenName = "RefreshToken";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(UserManager<ApplicationUser> userManager, AppDbContext context, IConfiguration config)
    {
        _userManager = userManager;
        _context = context;
        _config = config;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userManager.FindByNameAsync(request.Username);
        if (existing != null) return null;

        var user = new ApplicationUser { UserName = request.Username };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) return null;

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Action = "Register",
            Details = $"User '{user.UserName}' registered"
        });
        await _context.SaveChangesAsync();

        return await CreateAuthResponse(user);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return null;

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Action = "Login",
            Details = $"User '{user.UserName}' logged in"
        });
        await _context.SaveChangesAsync();

        return await CreateAuthResponse(user);
    }

    public async Task<bool> LogoutAsync(string jwt)
    {
        var stored = await _userManager.FindByLoginAsync(LoginProvider, AccessTokenName) ??
                     await FindUserByTokenAsync(AccessTokenName, jwt);

        if (stored == null) return false;

        await _userManager.RemoveAuthenticationTokenAsync(stored, LoginProvider, AccessTokenName);
        await _userManager.RemoveAuthenticationTokenAsync(stored, LoginProvider, RefreshTokenName);

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = stored.Id,
            Action = "Logout",
            Details = "Access and refresh tokens revoked via Identity"
        });
        await _context.SaveChangesAsync();
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
        var token = await _context.UserTokens
            .FirstOrDefaultAsync(t => t.LoginProvider == LoginProvider
                                   && t.Name == tokenName
                                   && t.Value == tokenValue);
        if (token == null) return null;
        return await _userManager.FindByIdAsync(token.UserId.ToString());
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
