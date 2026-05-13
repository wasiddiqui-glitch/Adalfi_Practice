using LibraryManagement.API.Data;
using LibraryManagement.API.DTOs;
using LibraryManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Services;

public class BookService : IBookService
{
    private readonly AppDbContext _context;

    public BookService(AppDbContext context)
    {
        _context = context;
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
                b.Copies.Count(c => !c.IsCheckedOut && !c.IsFaulty)
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
        return await _context.UserBooks
            .Where(ub => ub.UserId == userId)
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
                ub.DueDate
            ))
            .ToListAsync();
    }

    public async Task<bool> CheckoutBookAsync(int bookId, int userId)
    {
        var alreadyHas = await _context.UserBooks.AnyAsync(ub => ub.UserId == userId && ub.BookId == bookId);
        if (alreadyHas) return false;

        var copy = await _context.BookCopies
            .FirstOrDefaultAsync(c => c.BookId == bookId && !c.IsCheckedOut && !c.IsFaulty);
        if (copy == null) return false;

        copy.IsCheckedOut = true;
        _context.UserBooks.Add(new UserBook
        {
            UserId = userId,
            BookId = bookId,
            BookCopyId = copy.Id,
            CheckedOutAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14)
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReturnBookAsync(int bookId, int userId)
    {
        var userBook = await _context.UserBooks
            .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId);
        if (userBook == null) return false;

        var copy = await _context.BookCopies.FindAsync(userBook.BookCopyId);
        if (copy == null) return false;

        copy.IsCheckedOut = false;
        _context.UserBooks.Remove(userBook);
        await _context.SaveChangesAsync();
        return true;
    }

    // User reports the copy they currently have as faulty
    public async Task<bool> MarkFaultyAsync(int bookId, int userId, string reason)
    {
        var userBook = await _context.UserBooks
            .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId);
        if (userBook == null) return false;

        var copy = await _context.BookCopies.FindAsync(userBook.BookCopyId);
        if (copy == null) return false;

        copy.IsFaulty = true;
        copy.FaultyReason = reason;
        copy.IsCheckedOut = false;
        _context.UserBooks.Remove(userBook);
        await _context.SaveChangesAsync();
        return true;
    }

    // Admin marks a specific copy as faulty by copyId
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
