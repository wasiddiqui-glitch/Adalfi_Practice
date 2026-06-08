using LibraryManagement.Domain.Models;

namespace LibraryManagement.Application.Interfaces.Repositories;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllWithCopiesAsync();
    Task<Book?> GetByIdWithCopiesAsync(int id);
    Task<BookCopy?> GetAvailableCopyAsync(int bookId);
    Task<BookCopy?> GetCopyByIdAsync(int copyId);
    Task<bool> ExistsAsync(int bookId);
    void Add(Book book);
    void AddCopy(BookCopy copy);
    void Remove(Book book);
    void RemoveCopy(BookCopy copy);
}
