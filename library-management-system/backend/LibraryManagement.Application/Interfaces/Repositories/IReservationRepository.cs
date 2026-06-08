using LibraryManagement.Domain.Models;

namespace LibraryManagement.Application.Interfaces.Repositories;

public interface IReservationRepository
{
    Task<IEnumerable<Reservation>> GetActiveByUserAsync(int userId);
    Task<IEnumerable<Reservation>> GetActiveByBookAsync(int bookId);
    Task<Reservation?> GetActiveByUserAndBookAsync(int userId, int bookId);
    Task<Reservation?> GetByIdAsync(int id);
    Task<int> CountActiveByBookAsync(int bookId);
    void Add(Reservation reservation);
}
