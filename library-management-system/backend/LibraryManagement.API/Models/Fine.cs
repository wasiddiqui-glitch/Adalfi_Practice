namespace LibraryManagement.API.Models;

public class Fine
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public int UserBookId { get; set; }
    public UserBook UserBook { get; set; } = null!;
    public decimal Amount { get; set; }
    public int DaysOverdue { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
    public bool IsPaid => PaidAt != null;
}
