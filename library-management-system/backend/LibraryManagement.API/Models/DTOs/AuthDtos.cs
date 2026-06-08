namespace LibraryManagement.API.DTOs;

public record RegisterRequest(string Username, string Password);
public record LoginRequest(string Username, string Password);
public record AuthResponse(string Token, string RefreshToken, string Username, int UserId, bool IsAdmin);
public record RefreshRequest(int UserId, string RefreshToken);
