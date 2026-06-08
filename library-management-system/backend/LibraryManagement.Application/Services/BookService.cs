using LibraryManagement.Application.DTOs;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Application.Interfaces.Services;
using LibraryManagement.Domain.Models;
using Microsoft.Extensions.Configuration;

namespace LibraryManagement.Application.Services;

public class BookService : IBookService
{
    private readonly IUnitOfWork _uow;
    private readonly int _maxBooksPerUser;

    public BookService(IUnitOfWork uow, IConfiguration configuration)
    {
        _uow = uow;
        _maxBooksPerUser = configuration.GetValue<int>("MaxBooksPerUser", 5);
    }

    public async Task<IEnumerable<BookDto>> GetAvailableBooksAsync()
    {
        var books = await _uow.Books.GetAllWithCopiesAsync();
        return books.Select(b => new BookDto(
            b.Id, b.Title, b.Author, b.Genre, b.Description,
            b.Copies.Count,
            b.Copies.Count(c => !c.IsCheckedOut && !c.IsFaulty),
            b.Reservations.Count(r => r.CancelledAt == null && r.FulfilledAt == null)
        )).OrderBy(b => b.Title);
    }

    public async Task<IEnumerable<FaultyBookDto>> GetFaultyBooksAsync()
    {
        var books = await _uow.Books.GetAllWithCopiesAsync();
        return books
            .Where(b => b.Copies.Any(c => c.IsFaulty))
            .OrderBy(b => b.Title)
            .Select(b => new FaultyBookDto(
                b.Id, b.Title, b.Author, b.Genre,
                b.Copies.Where(c => c.IsFaulty)
                        .Select(c => new FaultyCopyDto(c.Id, c.CopyNumber, c.FaultyReason))
                        .ToList()
            ));
    }

    public async Task<IEnumerable<UserBookDto>> GetUserBooksAsync(int userId)
    {
        var now = DateTime.UtcNow;
        var userBooks = await _uow.UserBooks.GetActiveByUserAsync(userId);
        return userBooks.Select(ub => new UserBookDto(
            ub.Id, ub.BookId, ub.BookCopyId, ub.BookCopy.CopyNumber,
            ub.Book.Title, ub.Book.Author, ub.Book.Genre, ub.Book.Description,
            ub.CheckedOutAt, ub.DueDate, ub.DueDate < now
        ));
    }

    public async Task<IEnumerable<UserBookHistoryDto>> GetCheckoutHistoryAsync(int userId)
    {
        var history = await _uow.UserBooks.GetHistoryByUserAsync(userId);
        return history.Select(ub => new UserBookHistoryDto(
            ub.Id, ub.BookId, ub.BookCopyId, ub.BookCopy.CopyNumber,
            ub.Book.Title, ub.Book.Author, ub.Book.Genre, ub.Book.Description,
            ub.CheckedOutAt, ub.DueDate, ub.ReturnedAt!.Value
        ));
    }

    public async Task<(bool Success, string? Error)> CheckoutBookAsync(int bookId, int userId)
    {
        var activeCount = await _uow.UserBooks.CountActiveByUserAsync(userId);
        if (activeCount >= _maxBooksPerUser)
            return (false, $"Checkout limit reached. You can borrow at most {_maxBooksPerUser} books at a time.");

        var userReservation = await _uow.Reservations.GetActiveByUserAndBookAsync(userId, bookId);
        var copy = await _uow.Books.GetAvailableCopyAsync(bookId);
        if (copy == null) return (false, "No copies are currently available.");

        if (userReservation == null)
        {
            var physicalAvailable = (await _uow.Books.GetAllWithCopiesAsync())
                .FirstOrDefault(b => b.Id == bookId)?.Copies
                .Count(c => !c.IsCheckedOut && !c.IsFaulty) ?? 0;
            var pendingCount = await _uow.Reservations.CountActiveByBookAsync(bookId);

            if (physicalAvailable <= pendingCount)
                return (false, "Available copies are held for users in the queue. Join the waitlist to reserve your spot.");
        }

        copy.IsCheckedOut = true;
        _uow.UserBooks.Add(new UserBook
        {
            UserId = userId,
            BookId = bookId,
            BookCopyId = copy.Id,
            CheckedOutAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14)
        });

        if (userReservation != null)
            userReservation.FulfilledAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> ReturnBookAsync(int bookId, int userId)
    {
        var userBook = await _uow.UserBooks.GetActiveByUserAndBookAsync(userId, bookId);
        if (userBook == null) return false;

        var copy = await _uow.Books.GetCopyByIdAsync(userBook.BookCopyId);
        if (copy == null) return false;

        copy.IsCheckedOut = false;
        userBook.ReturnedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkFaultyAsync(int bookId, int userId, string reason)
    {
        var userBook = await _uow.UserBooks.GetActiveByUserAndBookAsync(userId, bookId);
        if (userBook == null) return false;

        var copy = await _uow.Books.GetCopyByIdAsync(userBook.BookCopyId);
        if (copy == null) return false;

        copy.IsFaulty = true;
        copy.FaultyReason = reason;
        copy.IsCheckedOut = false;
        userBook.ReturnedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkCopyFaultyAsync(int copyId, string reason)
    {
        var copy = await _uow.Books.GetCopyByIdAsync(copyId);
        if (copy == null) return false;
        copy.IsFaulty = true;
        copy.FaultyReason = reason;
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreCopyAsync(int copyId)
    {
        var copy = await _uow.Books.GetCopyByIdAsync(copyId);
        if (copy == null) return false;
        copy.IsFaulty = false;
        copy.FaultyReason = string.Empty;
        await _uow.SaveChangesAsync();
        return true;
    }
}
