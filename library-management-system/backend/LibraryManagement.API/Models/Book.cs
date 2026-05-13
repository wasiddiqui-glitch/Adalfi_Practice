namespace LibraryManagement.API.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<BookCopy> Copies { get; set; } = [];
    public ICollection<UserBook> UserBooks { get; set; } = [];
}
