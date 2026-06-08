using Microsoft.AspNetCore.Identity;

namespace LibraryManagement.Domain.Models;

public class ApplicationUser : IdentityUser<int>
{
    public bool IsAdmin { get; set; } = false;
    public ICollection<UserBook> UserBooks { get; set; } = [];
}
