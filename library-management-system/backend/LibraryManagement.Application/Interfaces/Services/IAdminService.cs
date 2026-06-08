using LibraryManagement.Application.DTOs;

namespace LibraryManagement.Application.Interfaces.Services;

public interface IAdminService
{
    Task<IEnumerable<AdminUserDto>> GetAllUsersWithBooksAsync();
    Task<IEnumerable<AdminCheckoutDto>> GetAllActiveCheckoutsAsync();
    Task<IEnumerable<AdminCheckoutDto>> GetOverdueCheckoutsAsync();
    Task<AdminBookDetailDto?> GetBookDetailAsync(int bookId);
    Task<AdminBookDetailDto> AddBookAsync(CreateBookRequest req);
    Task<AdminBookDetailDto?> UpdateBookAsync(int id, UpdateBookRequest req);
    Task<(bool Success, string? Error)> DeleteBookAsync(int id);
    Task<AdminCopyDto?> AddCopyAsync(int bookId);
    Task<(bool Success, string? Error)> DeleteCopyAsync(int id);
}
