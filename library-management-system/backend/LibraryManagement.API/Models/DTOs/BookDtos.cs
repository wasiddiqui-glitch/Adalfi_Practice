namespace LibraryManagement.API.DTOs;

public record BookDto(int Id, string Title, string Author, string Genre, string Description, int TotalCopies, int AvailableCopies);
public record UserBookDto(int Id, int BookId, int BookCopyId, int CopyNumber, string Title, string Author, string Genre, string Description, DateTime CheckedOutAt, DateTime DueDate);
public record FaultyCopyDto(int CopyId, int CopyNumber, string FaultyReason);
public record FaultyBookDto(int BookId, string Title, string Author, string Genre, List<FaultyCopyDto> FaultyCopies);
public record MarkFaultyRequest(string Reason);
public record AdminCheckoutDto(int UserBookId, int BookId, int BookCopyId, int CopyNumber, string BookTitle, string Username, DateTime CheckedOutAt, DateTime DueDate);
public record AdminUserDto(int Id, string Username, bool IsAdmin, List<AdminCheckoutDto> CurrentCheckouts);
