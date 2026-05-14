using LibraryManagement.API.Data;
using LibraryManagement.API.DTOs;
using LibraryManagement.API.Models;
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
        var now = DateTime.UtcNow;
        var users = await _context.Users.OrderBy(u => u.Username).ToListAsync();

        var activeCheckouts = await _context.UserBooks
            .Where(ub => ub.ReturnedAt == null)
            .Include(ub => ub.Book)
            .Include(ub => ub.BookCopy)
            .ToListAsync();

        var checkoutsByUser = activeCheckouts
            .GroupBy(ub => ub.UserId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return users.Select(u => new AdminUserDto(
            u.Id,
            u.Username,
            u.IsAdmin,
            checkoutsByUser.TryGetValue(u.Id, out var userCheckouts)
                ? userCheckouts.Select(ub => new AdminCheckoutDto(
                    ub.Id,
                    ub.BookId,
                    ub.BookCopyId,
                    ub.BookCopy.CopyNumber,
                    ub.Book.Title,
                    u.Username,
                    ub.CheckedOutAt,
                    ub.DueDate,
                    ub.DueDate < now
                  )).ToList()
                : []
        ));
    }

    public async Task<IEnumerable<AdminCheckoutDto>> GetAllActiveCheckoutsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.UserBooks
            .Where(ub => ub.ReturnedAt == null)
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
                ub.DueDate,
                ub.DueDate < now
            ))
            .ToListAsync();
    }

    public async Task<IEnumerable<AdminCheckoutDto>> GetOverdueCheckoutsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.UserBooks
            .Where(ub => ub.ReturnedAt == null && ub.DueDate < now)
            .Include(ub => ub.User)
            .Include(ub => ub.Book)
            .Include(ub => ub.BookCopy)
            .OrderBy(ub => ub.DueDate)
            .Select(ub => new AdminCheckoutDto(
                ub.Id,
                ub.BookId,
                ub.BookCopyId,
                ub.BookCopy.CopyNumber,
                ub.Book.Title,
                ub.User.Username,
                ub.CheckedOutAt,
                ub.DueDate,
                true
            ))
            .ToListAsync();
    }

    public async Task<AdminBookDetailDto?> GetBookDetailAsync(int bookId)
    {
        var book = await _context.Books
            .Include(b => b.Copies)
            .FirstOrDefaultAsync(b => b.Id == bookId);

        return book == null ? null : MapToBookDetail(book);
    }

    public async Task<AdminBookDetailDto> AddBookAsync(CreateBookRequest req)
    {
        var copies = Enumerable.Range(1, Math.Max(1, req.InitialCopies))
            .Select(n => new BookCopy { CopyNumber = n })
            .ToList();

        var book = new Book
        {
            Title = req.Title,
            Author = req.Author,
            Genre = req.Genre,
            Description = req.Description,
            Copies = copies
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        return MapToBookDetail(book);
    }

    public async Task<AdminBookDetailDto?> UpdateBookAsync(int id, UpdateBookRequest req)
    {
        var book = await _context.Books
            .Include(b => b.Copies)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null) return null;

        book.Title = req.Title;
        book.Author = req.Author;
        book.Genre = req.Genre;
        book.Description = req.Description;
        await _context.SaveChangesAsync();
        return MapToBookDetail(book);
    }

    public async Task<(bool Success, string? Error)> DeleteBookAsync(int id)
    {
        var book = await _context.Books
            .Include(b => b.Copies)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null) return (false, "Book not found.");

        if (book.Copies.Any(c => c.IsCheckedOut))
            return (false, "Cannot delete: some copies are currently checked out.");

        var copyIds = book.Copies.Select(c => c.Id).ToList();
        var relatedUserBooks = await _context.UserBooks
            .Where(ub => copyIds.Contains(ub.BookCopyId))
            .ToListAsync();
        _context.UserBooks.RemoveRange(relatedUserBooks);

        var relatedReservations = await _context.Reservations
            .Where(r => r.BookId == id)
            .ToListAsync();
        _context.Reservations.RemoveRange(relatedReservations);

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<AdminCopyDto?> AddCopyAsync(int bookId)
    {
        var book = await _context.Books
            .Include(b => b.Copies)
            .FirstOrDefaultAsync(b => b.Id == bookId);

        if (book == null) return null;

        var nextNumber = book.Copies.Any() ? book.Copies.Max(c => c.CopyNumber) + 1 : 1;
        var copy = new BookCopy { BookId = bookId, CopyNumber = nextNumber };
        _context.BookCopies.Add(copy);
        await _context.SaveChangesAsync();

        return new AdminCopyDto(copy.Id, copy.CopyNumber, copy.IsCheckedOut, copy.IsFaulty, copy.FaultyReason);
    }

    public async Task<(bool Success, string? Error)> DeleteCopyAsync(int id)
    {
        var copy = await _context.BookCopies.FindAsync(id);
        if (copy == null) return (false, "Copy not found.");
        if (copy.IsCheckedOut) return (false, "Cannot delete: copy is currently checked out.");

        var relatedUserBooks = await _context.UserBooks
            .Where(ub => ub.BookCopyId == id)
            .ToListAsync();
        _context.UserBooks.RemoveRange(relatedUserBooks);

        _context.BookCopies.Remove(copy);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    private static AdminBookDetailDto MapToBookDetail(Book book) =>
        new(book.Id, book.Title, book.Author, book.Genre, book.Description,
            book.Copies
                .OrderBy(c => c.CopyNumber)
                .Select(c => new AdminCopyDto(c.Id, c.CopyNumber, c.IsCheckedOut, c.IsFaulty, c.FaultyReason))
                .ToList());
}
