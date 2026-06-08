using LibraryManagement.Application.DTOs;
using LibraryManagement.Application.Interfaces;
using LibraryManagement.Application.Interfaces.Services;
using LibraryManagement.Domain.Models;

namespace LibraryManagement.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IUnitOfWork _uow;

    public ReservationService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<UserReservationDto>> GetUserReservationsAsync(int userId)
    {
        var userReservations = (await _uow.Reservations.GetActiveByUserAsync(userId)).ToList();
        if (!userReservations.Any()) return [];

        var result = new List<UserReservationDto>();
        foreach (var r in userReservations)
        {
            var queue = (await _uow.Reservations.GetActiveByBookAsync(r.BookId)).ToList();
            var position = queue.FindIndex(q => q.Id == r.Id) + 1;
            var book = r.Book;
            var physicalAvailable = book.Copies.Count(c => !c.IsCheckedOut && !c.IsFaulty);
            result.Add(new UserReservationDto(
                r.Id, r.BookId, book.Title, book.Author, book.Genre,
                r.ReservedAt, position, position == 1 && physicalAvailable > 0
            ));
        }
        return result;
    }

    public async Task<(bool Success, string? Error)> ReserveAsync(int bookId, int userId)
    {
        var bookExists = await _uow.Books.ExistsAsync(bookId);
        if (!bookExists) return (false, "Book not found.");

        var alreadyCheckedOut = await _uow.UserBooks.GetActiveByUserAndBookAsync(userId, bookId);
        if (alreadyCheckedOut != null) return (false, "You already have this book checked out.");

        var alreadyReserved = await _uow.Reservations.GetActiveByUserAndBookAsync(userId, bookId);
        if (alreadyReserved != null) return (false, "You already have this book in your queue.");

        _uow.Reservations.Add(new Reservation { UserId = userId, BookId = bookId });
        await _uow.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> CancelAsync(int reservationId, int userId)
    {
        var reservation = await _uow.Reservations.GetByIdAsync(reservationId);
        if (reservation == null || reservation.UserId != userId
            || reservation.CancelledAt != null || reservation.FulfilledAt != null)
            return false;

        reservation.CancelledAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return true;
    }
}
