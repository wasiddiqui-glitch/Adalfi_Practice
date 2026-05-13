namespace LibraryManagement.API.Models;

public class Book
{
    public int Id {get; set;}
    public string Title { get; set; } = string.Empty;
    public string CountryOverview { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public ICollection<UserBook> UserBooks { get; set; } = [];
}
