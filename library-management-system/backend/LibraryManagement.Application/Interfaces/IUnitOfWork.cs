using LibraryManagement.Application.Interfaces.Repositories;

namespace LibraryManagement.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IBookRepository Books { get; }
    IUserBookRepository UserBooks { get; }
    IReservationRepository Reservations { get; }
    Task<int> SaveChangesAsync();
}
