namespace LibraryManagement.API.DTOs;

public record BookDto(int Id, string Title, string CountryOverview, bool IsAvailable);
public record UserBookDto(int Id, int BookId, string Title, string CountryOverview, DateTime CheckedOutAt);
