using LibraryManagement.API.Data;
using LibraryManagement.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;

    public AdminService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AdminUserDto>> GetAllUsersWithBooksAsync()
    {
        var users = await _context.Users
            .Include(u => u.UserBooks)
                .ThenInclude(ub => ub.Book)
            .Include(u => u.UserBooks)
                .ThenInclude(ub => ub.BookCopy)
            .OrderBy(u => u.Username)
            .ToListAsync();

        return users.Select(u => new AdminUserDto(
            u.Id,
            u.Username,
            u.IsAdmin,
            u.UserBooks.Select(ub => new AdminCheckoutDto(
                ub.Id,
                ub.BookId,
                ub.BookCopyId,
                ub.BookCopy.CopyNumber,
                ub.Book.Title,
                u.Username,
                ub.CheckedOutAt,
                ub.DueDate
            )).ToList()
        ));
    }

    public async Task<IEnumerable<AdminCheckoutDto>> GetAllActiveCheckoutsAsync()
    {
        return await _context.UserBooks
            .Include(ub => ub.User)
            .Include(ub => ub.Book)
            .Include(ub => ub.BookCopy)
            .OrderByDescending(ub => ub.CheckedOutAt)
            .Select(ub => new AdminCheckoutDto(
                ub.Id,
                ub.BookId,
                ub.BookCopyId,
                ub.BookCopy.CopyNumber,
                ub.Book.Title,
                ub.User.Username,
                ub.CheckedOutAt,
                ub.DueDate
            ))
            .ToListAsync();
    }
}
