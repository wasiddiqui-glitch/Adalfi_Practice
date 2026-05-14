using LibraryManagement.API.DTOs;

namespace LibraryManagement.API.Services;

public interface IReservationService
{
    Task<IEnumerable<UserReservationDto>> GetUserReservationsAsync(int userId);
    Task<(bool Success, string? Error)> ReserveAsync(int bookId, int userId);
    Task<bool> CancelAsync(int reservationId, int userId);
}
