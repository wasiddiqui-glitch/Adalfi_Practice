using LibraryManagement.API.DTOs;

namespace LibraryManagement.API.Services;

public interface IBookService
{
    Task<IEnumerable<BookDto>> GetAvailableBooksAsync();
    Task<IEnumerable<FaultyBookDto>> GetFaultyBooksAsync();
    Task<IEnumerable<UserBookDto>> GetUserBooksAsync(int userId);
    Task<IEnumerable<UserBookHistoryDto>> GetCheckoutHistoryAsync(int userId);
    Task<(bool Success, string? Error)> CheckoutBookAsync(int bookId, int userId);
    Task<bool> ReturnBookAsync(int bookId, int userId);
    Task<bool> MarkFaultyAsync(int bookId, int userId, string reason);
    Task<bool> MarkCopyFaultyAsync(int copyId, string reason);
    Task<bool> RestoreCopyAsync(int copyId);
}
