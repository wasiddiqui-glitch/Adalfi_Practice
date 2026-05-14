using LibraryManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    // GET /api/reservations — current user's active reservations with queue positions
    [HttpGet]
    public async Task<IActionResult> GetMyReservations()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var reservations = await _reservationService.GetUserReservationsAsync(userId);
        return Ok(reservations);
    }

    // POST /api/reservations/{bookId} — join the waitlist for a book
    [HttpPost("{bookId}")]
    public async Task<IActionResult> Reserve(int bookId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, error) = await _reservationService.ReserveAsync(bookId, userId);
        if (!success) return BadRequest(new { message = error });
        return Ok(new { message = "You've been added to the waitlist." });
    }

    // DELETE /api/reservations/{id} — cancel a reservation
    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _reservationService.CancelAsync(id, userId);
        if (!success) return NotFound(new { message = "Reservation not found." });
        return Ok(new { message = "Reservation cancelled." });
    }
}
