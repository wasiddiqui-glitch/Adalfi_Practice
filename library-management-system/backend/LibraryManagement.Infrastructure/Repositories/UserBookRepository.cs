using LibraryManagement.Application.Interfaces.Repositories;
using LibraryManagement.Domain.Models;
using LibraryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Repositories;

public class UserBookRepository : IUserBookRepository
{
    private readonly AppDbContext _context;

    public UserBookRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserBook>> GetActiveByUserAsync(int userId) =>
        await _context.UserBooks
            .Where(ub => ub.UserId == userId && ub.ReturnedAt == null)
            .Include(ub => ub.Book)
            .Include(ub => ub.BookCopy)
            .ToListAsync();

    public async Task<IEnumerable<UserBook>> GetHistoryByUserAsync(int userId) =>
        await _context.UserBooks
            .Where(ub => ub.UserId == userId && ub.ReturnedAt != null)
            .Include(ub => ub.Book)
            .Include(ub => ub.BookCopy)
            .OrderByDescending(ub => ub.ReturnedAt)
            .ToListAsync();

    public async Task<IEnumerable<UserBook>> GetAllActiveAsync() =>
        await _context.UserBooks
            .Where(ub => ub.ReturnedAt == null)
            .Include(ub => ub.User)
            .Include(ub => ub.Book)
            .Include(ub => ub.BookCopy)
            .ToListAsync();

    public async Task<IEnumerable<UserBook>> GetAllOverdueAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.UserBooks
            .Where(ub => ub.ReturnedAt == null && ub.DueDate < now)
            .Include(ub => ub.User)
            .Include(ub => ub.Book)
            .Include(ub => ub.BookCopy)
            .OrderBy(ub => ub.DueDate)
            .ToListAsync();
    }

    public async Task<UserBook?> GetActiveByUserAndBookAsync(int userId, int bookId) =>
        await _context.UserBooks
            .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId && ub.ReturnedAt == null);

    public async Task<int> CountActiveByUserAsync(int userId) =>
        await _context.UserBooks.CountAsync(ub => ub.UserId == userId && ub.ReturnedAt == null);

    public void Add(UserBook userBook) => _context.UserBooks.Add(userBook);
}
