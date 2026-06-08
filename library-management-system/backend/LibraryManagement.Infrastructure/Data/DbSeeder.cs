using LibraryManagement.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace LibraryManagement.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        if (!context.Users.Any(u => u.IsAdmin))
        {
            var admin = new ApplicationUser { UserName = "admin", IsAdmin = true };
            await userManager.CreateAsync(admin, "Admin@1234");
        }

        if (context.Books.Any()) return;

        var bookData = new[]
        {
            ("Animal Farm", "George Orwell", "Political Satire", "A short allegorical novel in which farm animals overthrow their human farmer."),
            ("Brave New World", "Aldous Huxley", "Dystopian Fiction", "A futuristic World State where citizens are manufactured through genetic engineering."),
            ("Crime and Punishment", "Fyodor Dostoevsky", "Classic Fiction", "A psychological thriller following Raskolnikov, a destitute student in St. Petersburg."),
            ("Dune", "Frank Herbert", "Science Fiction", "Set in the distant future on the desert planet Arrakis."),
            ("Harry Potter and the Philosopher's Stone", "J.K. Rowling", "Fantasy", "Eleven-year-old Harry Potter discovers he is a wizard."),
            ("Nineteen Eighty-Four", "George Orwell", "Dystopian Fiction", "In the totalitarian super-state of Oceania, Winston Smith secretly rebels."),
            ("Pride and Prejudice", "Jane Austen", "Classic Romance", "The witty Elizabeth Bennet and the proud Mr. Darcy clash and fall in love."),
            ("The Alchemist", "Paulo Coelho", "Philosophical Fiction", "Santiago, an Andalusian shepherd boy, dreams of a treasure near the pyramids."),
            ("To Kill a Mockingbird", "Harper Lee", "Classic Fiction", "Told through the eyes of young Scout Finch in Depression-era Alabama."),
            ("Wuthering Heights", "Emily Brontë", "Gothic Romance", "The foundling Heathcliff and Catherine fall deeply in love on the Yorkshire moors."),
        };

        foreach (var (title, author, genre, description) in bookData)
        {
            context.Books.Add(new Book
            {
                Title = title, Author = author, Genre = genre, Description = description,
                Copies = Enumerable.Range(1, 5).Select(n => new BookCopy { CopyNumber = n }).ToList()
            });
        }

        await context.SaveChangesAsync();
    }
}
