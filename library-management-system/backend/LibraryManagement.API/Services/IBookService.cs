using LibraryManagement.API.DTOs;

namespace LibraryManagement.API.Services;

public interface IBookService
{
    Task<IEnumerable<BookDto>> GetAvailableBooksAsync();
    Task<IEnumerable<UserBookDto>> GetUserBooksAsync(int userId);
    Task<bool> CheckoutBookAsync(int bookId, int userId);
    Task<bool> ReturnBookAsync(int bookId, int userId);
}
