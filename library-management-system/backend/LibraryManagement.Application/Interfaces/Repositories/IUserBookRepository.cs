using LibraryManagement.Domain.Models;

namespace LibraryManagement.Application.Interfaces.Repositories;

public interface IUserBookRepository
{
    Task<IEnumerable<UserBook>> GetActiveByUserAsync(int userId);
    Task<IEnumerable<UserBook>> GetHistoryByUserAsync(int userId);
    Task<IEnumerable<UserBook>> GetAllActiveAsync();
    Task<IEnumerable<UserBook>> GetAllOverdueAsync();
    Task<UserBook?> GetActiveByUserAndBookAsync(int userId, int bookId);
    Task<int> CountActiveByUserAsync(int userId);
    void Add(UserBook userBook);
}
