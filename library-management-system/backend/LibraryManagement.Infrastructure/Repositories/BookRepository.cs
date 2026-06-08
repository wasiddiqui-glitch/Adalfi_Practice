using LibraryManagement.Application.Interfaces.Repositories;
using LibraryManagement.Domain.Models;
using LibraryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Repositories;

public class BookRepository : IBookRepository
{
    private readonly AppDbContext _context;

    public BookRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Book>> GetAllWithCopiesAsync() =>
        await _context.Books
            .Include(b => b.Copies)
            .Include(b => b.Reservations)
            .OrderBy(b => b.Title)
            .ToListAsync();

    public async Task<Book?> GetByIdWithCopiesAsync(int id) =>
        await _context.Books
            .Include(b => b.Copies)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<BookCopy?> GetAvailableCopyAsync(int bookId) =>
        await _context.BookCopies
            .FirstOrDefaultAsync(c => c.BookId == bookId && !c.IsCheckedOut && !c.IsFaulty);

    public async Task<BookCopy?> GetCopyByIdAsync(int copyId) =>
        await _context.BookCopies.FindAsync(copyId);

    public async Task<bool> ExistsAsync(int bookId) =>
        await _context.Books.AnyAsync(b => b.Id == bookId);

    public void Add(Book book) => _context.Books.Add(book);
    public void AddCopy(BookCopy copy) => _context.BookCopies.Add(copy);
    public void Remove(Book book) => _context.Books.Remove(book);
    public void RemoveCopy(BookCopy copy) => _context.BookCopies.Remove(copy);
}
