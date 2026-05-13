namespace LibraryManagement.API.Models;

public class BookCopy
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public int CopyNumber { get; set; }
    public bool IsCheckedOut { get; set; } = false;
    public bool IsFaulty { get; set; } = false;
    public string FaultyReason { get; set; } = string.Empty;
}
