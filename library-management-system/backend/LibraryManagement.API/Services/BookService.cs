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
            .Where(b => b.IsAvailable)
            .Select(b => new BookDto(b.Id, b.Title, b.CountryOverview, b.IsAvailable))
            .ToListAsync();
    }

    public async Task<IEnumerable<UserBookDto>> GetUserBooksAsync(int userId)
    {
        return await _context.UserBooks
            .Where(ub => ub.UserId == userId)
            .Include(ub => ub.Book)
            .Select(ub => new UserBookDto(ub.Id, ub.BookId, ub.Book.Title, ub.Book.CountryOverview, ub.CheckedOutAt))
            .ToListAsync();
    }

    public async Task<bool> CheckoutBookAsync(int bookId, int userId)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book == null || !book.IsAvailable)
            return false;

        var alreadyHas = await _context.UserBooks.AnyAsync(ub => ub.UserId == userId && ub.BookId == bookId);
        if (alreadyHas) return false;

        book.IsAvailable = false;
        _context.UserBooks.Add(new UserBook { UserId = userId, BookId = bookId });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReturnBookAsync(int bookId, int userId)
    {
        var userBook = await _context.UserBooks
            .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId);

        if (userBook == null) return false;

        var book = await _context.Books.FindAsync(bookId);
        if (book == null) return false;

        book.IsAvailable = true;
        _context.UserBooks.Remove(userBook);
        await _context.SaveChangesAsync();
        return true;
    }
}
