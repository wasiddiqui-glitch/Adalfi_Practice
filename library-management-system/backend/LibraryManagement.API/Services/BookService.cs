using LibraryManagement.API.Data;
using LibraryManagement.API.DTOs;
using LibraryManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Services;

public class BookService : IBookService
{
    private readonly AppDbContext _context;
    private readonly int _maxBooksPerUser;

    public BookService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _maxBooksPerUser = configuration.GetValue<int>("MaxBooksPerUser", 5);
    }

    public async Task<IEnumerable<BookDto>> GetAvailableBooksAsync()
    {
        return await _context.Books
            .OrderBy(b => b.Title)
            .Select(b => new BookDto(
                b.Id,
                b.Title,
                b.Author,
                b.Genre,
                b.Description,
                b.Copies.Count,
                b.Copies.Count(c => !c.IsCheckedOut && !c.IsFaulty),
                b.Reservations.Count(r => r.CancelledAt == null && r.FulfilledAt == null)
            ))
            .ToListAsync();
    }

    public async Task<IEnumerable<FaultyBookDto>> GetFaultyBooksAsync()
    {
        var books = await _context.Books
            .Include(b => b.Copies.Where(c => c.IsFaulty))
            .Where(b => b.Copies.Any(c => c.IsFaulty))
            .OrderBy(b => b.Title)
            .ToListAsync();

        return books.Select(b => new FaultyBookDto(
            b.Id,
            b.Title,
            b.Author,
            b.Genre,
            b.Copies.Where(c => c.IsFaulty)
                    .Select(c => new FaultyCopyDto(c.Id, c.CopyNumber, c.FaultyReason))
                    .ToList()
        ));
    }

    public async Task<IEnumerable<UserBookDto>> GetUserBooksAsync(int userId)
    {
        var now = DateTime.UtcNow;
        return await _context.UserBooks
            .Where(ub => ub.UserId == userId && ub.ReturnedAt == null)
            .Include(ub => ub.Book)
            .Include(ub => ub.BookCopy)
            .Select(ub => new UserBookDto(
                ub.Id,
                ub.BookId,
                ub.BookCopyId,
                ub.BookCopy.CopyNumber,
                ub.Book.Title,
                ub.Book.Author,
                ub.Book.Genre,
                ub.Book.Description,
                ub.CheckedOutAt,
                ub.DueDate,
                ub.DueDate < now
            ))
            .ToListAsync();
    }

    public async Task<IEnumerable<UserBookHistoryDto>> GetCheckoutHistoryAsync(int userId)
    {
        return await _context.UserBooks
            .Where(ub => ub.UserId == userId && ub.ReturnedAt != null)
            .Include(ub => ub.Book)
            .Include(ub => ub.BookCopy)
            .OrderByDescending(ub => ub.ReturnedAt)
            .Select(ub => new UserBookHistoryDto(
                ub.Id,
                ub.BookId,
                ub.BookCopyId,
                ub.BookCopy.CopyNumber,
                ub.Book.Title,
                ub.Book.Author,
                ub.Book.Genre,
                ub.Book.Description,
                ub.CheckedOutAt,
                ub.DueDate,
                ub.ReturnedAt!.Value
            ))
            .ToListAsync();
    }

    public async Task<(bool Success, string? Error)> CheckoutBookAsync(int bookId, int userId)
    {
        var activeCount = await _context.UserBooks.CountAsync(ub =>
            ub.UserId == userId && ub.ReturnedAt == null);
        if (activeCount >= _maxBooksPerUser)
            return (false, $"Checkout limit reached. You can borrow at most {_maxBooksPerUser} books at a time.");

        var userReservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.UserId == userId && r.BookId == bookId
                && r.CancelledAt == null && r.FulfilledAt == null);

        var copy = await _context.BookCopies
            .FirstOrDefaultAsync(c => c.BookId == bookId && !c.IsCheckedOut && !c.IsFaulty);
        if (copy == null) return (false, "No copies are currently available.");

        if (userReservation == null)
        {
            var physicalAvailable = await _context.BookCopies.CountAsync(c =>
                c.BookId == bookId && !c.IsCheckedOut && !c.IsFaulty);
            var pendingCount = await _context.Reservations.CountAsync(r =>
                r.BookId == bookId && r.CancelledAt == null && r.FulfilledAt == null);

            if (physicalAvailable <= pendingCount)
                return (false, "Available copies are held for users in the queue. Join the waitlist to reserve your spot.");
        }

        copy.IsCheckedOut = true;
        _context.UserBooks.Add(new UserBook
        {
            UserId = userId,
            BookId = bookId,
            BookCopyId = copy.Id,
            CheckedOutAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14)
        });

        if (userReservation != null)
            userReservation.FulfilledAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> ReturnBookAsync(int bookId, int userId)
    {
        var userBook = await _context.UserBooks
            .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId && ub.ReturnedAt == null);
        if (userBook == null) return false;

        var copy = await _context.BookCopies.FindAsync(userBook.BookCopyId);
        if (copy == null) return false;

        copy.IsCheckedOut = false;
        userBook.ReturnedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkFaultyAsync(int bookId, int userId, string reason)
    {
        var userBook = await _context.UserBooks
            .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId && ub.ReturnedAt == null);
        if (userBook == null) return false;

        var copy = await _context.BookCopies.FindAsync(userBook.BookCopyId);
        if (copy == null) return false;

        copy.IsFaulty = true;
        copy.FaultyReason = reason;
        copy.IsCheckedOut = false;
        userBook.ReturnedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkCopyFaultyAsync(int copyId, string reason)
    {
        var copy = await _context.BookCopies.FindAsync(copyId);
        if (copy == null) return false;

        copy.IsFaulty = true;
        copy.FaultyReason = reason;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreCopyAsync(int copyId)
    {
        var copy = await _context.BookCopies.FindAsync(copyId);
        if (copy == null) return false;

        copy.IsFaulty = false;
        copy.FaultyReason = string.Empty;
        await _context.SaveChangesAsync();
        return true;
    }
}
