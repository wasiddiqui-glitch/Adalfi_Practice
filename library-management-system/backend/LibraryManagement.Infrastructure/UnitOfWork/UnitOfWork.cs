using LibraryManagement.Application.Interfaces;
using LibraryManagement.Application.Interfaces.Repositories;
using LibraryManagement.Infrastructure.Data;
using LibraryManagement.Infrastructure.Repositories;

namespace LibraryManagement.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IBookRepository Books { get; }
    public IUserBookRepository UserBooks { get; }
    public IReservationRepository Reservations { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Books = new BookRepository(context);
        UserBooks = new UserBookRepository(context);
        Reservations = new ReservationRepository(context);
    }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}
