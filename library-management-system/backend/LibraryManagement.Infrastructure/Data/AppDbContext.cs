using LibraryManagement.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookCopy> BookCopies => Set<BookCopy>();
    public DbSet<UserBook> UserBooks => Set<UserBook>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Fine> Fines => Set<Fine>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BookCopy>()
            .HasOne(bc => bc.Book).WithMany(b => b.Copies).HasForeignKey(bc => bc.BookId);

        modelBuilder.Entity<UserBook>()
            .HasOne(ub => ub.User).WithMany(u => u.UserBooks).HasForeignKey(ub => ub.UserId);

        modelBuilder.Entity<UserBook>()
            .HasOne(ub => ub.Book).WithMany(b => b.UserBooks).HasForeignKey(ub => ub.BookId);

        modelBuilder.Entity<UserBook>()
            .HasOne(ub => ub.BookCopy).WithMany().HasForeignKey(ub => ub.BookCopyId);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Book).WithMany(b => b.Reservations).HasForeignKey(r => r.BookId);
    }
}
