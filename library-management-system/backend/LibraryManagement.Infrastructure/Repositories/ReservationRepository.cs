using LibraryManagement.Application.Interfaces.Repositories;
using LibraryManagement.Domain.Models;
using LibraryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _context;

    public ReservationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Reservation>> GetActiveByUserAsync(int userId) =>
        await _context.Reservations
            .Where(r => r.UserId == userId && r.CancelledAt == null && r.FulfilledAt == null)
            .Include(r => r.Book).ThenInclude(b => b.Copies)
            .OrderBy(r => r.ReservedAt)
            .ToListAsync();

    public async Task<IEnumerable<Reservation>> GetActiveByBookAsync(int bookId) =>
        await _context.Reservations
            .Where(r => r.BookId == bookId && r.CancelledAt == null && r.FulfilledAt == null)
            .OrderBy(r => r.ReservedAt)
            .ToListAsync();

    public async Task<Reservation?> GetActiveByUserAndBookAsync(int userId, int bookId) =>
        await _context.Reservations
            .FirstOrDefaultAsync(r => r.UserId == userId && r.BookId == bookId
                && r.CancelledAt == null && r.FulfilledAt == null);

    public async Task<Reservation?> GetByIdAsync(int id) =>
        await _context.Reservations.FindAsync(id);

    public async Task<int> CountActiveByBookAsync(int bookId) =>
        await _context.Reservations
            .CountAsync(r => r.BookId == bookId && r.CancelledAt == null && r.FulfilledAt == null);

    public void Add(Reservation reservation) => _context.Reservations.Add(reservation);
}
