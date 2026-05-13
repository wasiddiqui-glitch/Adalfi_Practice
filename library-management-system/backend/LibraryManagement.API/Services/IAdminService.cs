using LibraryManagement.API.DTOs;

namespace LibraryManagement.API.Services;

public interface IAdminService
{
    Task<IEnumerable<AdminUserDto>> GetAllUsersWithBooksAsync();
    Task<IEnumerable<AdminCheckoutDto>> GetAllActiveCheckoutsAsync();
}
