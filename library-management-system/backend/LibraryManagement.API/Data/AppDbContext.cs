using LibraryManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookCopy> BookCopies => Set<BookCopy>();
    public DbSet<UserBook> UserBooks => Set<UserBook>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookCopy>()
            .HasOne(bc => bc.Book)
            .WithMany(b => b.Copies)
            .HasForeignKey(bc => bc.BookId);

        modelBuilder.Entity<UserBook>()
            .HasOne(ub => ub.User)
            .WithMany(u => u.UserBooks)
            .HasForeignKey(ub => ub.UserId);

        modelBuilder.Entity<UserBook>()
            .HasOne(ub => ub.Book)
            .WithMany(b => b.UserBooks)
            .HasForeignKey(ub => ub.BookId);

        modelBuilder.Entity<UserBook>()
            .HasOne(ub => ub.BookCopy)
            .WithMany()
            .HasForeignKey(ub => ub.BookCopyId);
    }
}
