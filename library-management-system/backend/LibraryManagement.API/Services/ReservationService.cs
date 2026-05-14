using LibraryManagement.API.Data;
using LibraryManagement.API.DTOs;
using LibraryManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Services;

public class ReservationService : IReservationService
{
    private readonly AppDbContext _context;

    public ReservationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserReservationDto>> GetUserReservationsAsync(int userId)
    {
        var userReservations = await _context.Reservations
            .Where(r => r.UserId == userId && r.CancelledAt == null && r.FulfilledAt == null)
            .Include(r => r.Book)
                .ThenInclude(b => b.Copies)
            .OrderBy(r => r.ReservedAt)
            .ToListAsync();

        if (!userReservations.Any()) return [];

        var bookIds = userReservations.Select(r => r.BookId).Distinct().ToList();
        var allQueuedReservations = await _context.Reservations
            .Where(r => bookIds.Contains(r.BookId) && r.CancelledAt == null && r.FulfilledAt == null)
            .OrderBy(r => r.ReservedAt)
            .ToListAsync();

        var queueByBook = allQueuedReservations
            .GroupBy(r => r.BookId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return userReservations.Select(r =>
        {
            var queue = queueByBook[r.BookId];
            var position = queue.FindIndex(q => q.Id == r.Id) + 1;
            var physicalAvailable = r.Book.Copies.Count(c => !c.IsCheckedOut && !c.IsFaulty);
            var canCheckout = position == 1 && physicalAvailable > 0;
            return new UserReservationDto(r.Id, r.BookId, r.Book.Title, r.Book.Author, r.Book.Genre, r.ReservedAt, position, canCheckout);
        });
    }

    public async Task<(bool Success, string? Error)> ReserveAsync(int bookId, int userId)
    {
        var bookExists = await _context.Books.AnyAsync(b => b.Id == bookId);
        if (!bookExists) return (false, "Book not found.");

        var alreadyCheckedOut = await _context.UserBooks.AnyAsync(ub =>
            ub.UserId == userId && ub.BookId == bookId && ub.ReturnedAt == null);
        if (alreadyCheckedOut) return (false, "You already have this book checked out.");

        var alreadyReserved = await _context.Reservations.AnyAsync(r =>
            r.UserId == userId && r.BookId == bookId && r.CancelledAt == null && r.FulfilledAt == null);
        if (alreadyReserved) return (false, "You already have this book in your queue.");

        _context.Reservations.Add(new Reservation { UserId = userId, BookId = bookId });
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> CancelAsync(int reservationId, int userId)
    {
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId
                && r.CancelledAt == null && r.FulfilledAt == null);
        if (reservation == null) return false;

        reservation.CancelledAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
