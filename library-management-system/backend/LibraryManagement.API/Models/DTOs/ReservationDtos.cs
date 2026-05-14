namespace LibraryManagement.API.DTOs;

public record UserReservationDto(int Id, int BookId, string BookTitle, string BookAuthor, string BookGenre, DateTime ReservedAt, int QueuePosition, bool CanCheckout);
