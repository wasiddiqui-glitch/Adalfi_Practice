namespace LibraryManagement.API.Models;

public class UserBook
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public int BookCopyId { get; set; }
    public BookCopy BookCopy { get; set; } = null!;
    public DateTime CheckedOutAt { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
}
