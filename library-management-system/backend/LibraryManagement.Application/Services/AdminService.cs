using LibraryManagement.Application.DTOs;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Application.Interfaces.Services;
using LibraryManagement.Domain.Models;

namespace LibraryManagement.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _uow;

    public AdminService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<AdminUserDto>> GetAllUsersWithBooksAsync()
    {
        var now = DateTime.UtcNow;
        var allActive = await _uow.UserBooks.GetAllActiveAsync();
        var checkoutsByUser = allActive.GroupBy(ub => ub.UserId).ToDictionary(g => g.Key, g => g.ToList());

        var books = await _uow.Books.GetAllWithCopiesAsync();
        return checkoutsByUser.Select(kvp => new AdminUserDto(
            kvp.Key,
            kvp.Value.First().User.UserName ?? string.Empty,
            kvp.Value.First().User.IsAdmin,
            kvp.Value.Select(ub => new AdminCheckoutDto(
                ub.Id, ub.BookId, ub.BookCopyId, ub.BookCopy.CopyNumber,
                ub.Book.Title, ub.User.UserName ?? string.Empty,
                ub.CheckedOutAt, ub.DueDate, ub.DueDate < now
            )).ToList()
        ));
    }

    public async Task<IEnumerable<AdminCheckoutDto>> GetAllActiveCheckoutsAsync()
    {
        var now = DateTime.UtcNow;
        var active = await _uow.UserBooks.GetAllActiveAsync();
        return active.Select(ub => new AdminCheckoutDto(
            ub.Id, ub.BookId, ub.BookCopyId, ub.BookCopy.CopyNumber,
            ub.Book.Title, ub.User.UserName ?? string.Empty,
            ub.CheckedOutAt, ub.DueDate, ub.DueDate < now
        )).OrderByDescending(c => c.CheckedOutAt);
    }

    public async Task<IEnumerable<AdminCheckoutDto>> GetOverdueCheckoutsAsync()
    {
        var overdue = await _uow.UserBooks.GetAllOverdueAsync();
        return overdue.Select(ub => new AdminCheckoutDto(
            ub.Id, ub.BookId, ub.BookCopyId, ub.BookCopy.CopyNumber,
            ub.Book.Title, ub.User.UserName ?? string.Empty,
            ub.CheckedOutAt, ub.DueDate, true
        )).OrderBy(c => c.DueDate);
    }

    public async Task<AdminBookDetailDto?> GetBookDetailAsync(int bookId)
    {
        var book = await _uow.Books.GetByIdWithCopiesAsync(bookId);
        return book == null ? null : MapToBookDetail(book);
    }

    public async Task<AdminBookDetailDto> AddBookAsync(CreateBookRequest req)
    {
        var book = new Book
        {
            Title = req.Title, Author = req.Author,
            Genre = req.Genre, Description = req.Description,
            Copies = Enumerable.Range(1, Math.Max(1, req.InitialCopies))
                .Select(n => new BookCopy { CopyNumber = n }).ToList()
        };
        _uow.Books.Add(book);
        await _uow.SaveChangesAsync();
        return MapToBookDetail(book);
    }

    public async Task<AdminBookDetailDto?> UpdateBookAsync(int id, UpdateBookRequest req)
    {
        var book = await _uow.Books.GetByIdWithCopiesAsync(id);
        if (book == null) return null;
        book.Title = req.Title; book.Author = req.Author;
        book.Genre = req.Genre; book.Description = req.Description;
        await _uow.SaveChangesAsync();
        return MapToBookDetail(book);
    }

    public async Task<(bool Success, string? Error)> DeleteBookAsync(int id)
    {
        var book = await _uow.Books.GetByIdWithCopiesAsync(id);
        if (book == null) return (false, "Book not found.");
        if (book.Copies.Any(c => c.IsCheckedOut))
            return (false, "Cannot delete: some copies are currently checked out.");
        _uow.Books.Remove(book);
        await _uow.SaveChangesAsync();
        return (true, null);
    }

    public async Task<AdminCopyDto?> AddCopyAsync(int bookId)
    {
        var book = await _uow.Books.GetByIdWithCopiesAsync(bookId);
        if (book == null) return null;
        var nextNumber = book.Copies.Any() ? book.Copies.Max(c => c.CopyNumber) + 1 : 1;
        var copy = new BookCopy { BookId = bookId, CopyNumber = nextNumber };
        _uow.Books.AddCopy(copy);
        await _uow.SaveChangesAsync();
        return new AdminCopyDto(copy.Id, copy.CopyNumber, copy.IsCheckedOut, copy.IsFaulty, copy.FaultyReason);
    }

    public async Task<(bool Success, string? Error)> DeleteCopyAsync(int id)
    {
        var copy = await _uow.Books.GetCopyByIdAsync(id);
        if (copy == null) return (false, "Copy not found.");
        if (copy.IsCheckedOut) return (false, "Cannot delete: copy is currently checked out.");
        _uow.Books.RemoveCopy(copy);
        await _uow.SaveChangesAsync();
        return (true, null);
    }

    private static AdminBookDetailDto MapToBookDetail(Book book) =>
        new(book.Id, book.Title, book.Author, book.Genre, book.Description,
            book.Copies.OrderBy(c => c.CopyNumber)
                .Select(c => new AdminCopyDto(c.Id, c.CopyNumber, c.IsCheckedOut, c.IsFaulty, c.FaultyReason))
                .ToList());
}
