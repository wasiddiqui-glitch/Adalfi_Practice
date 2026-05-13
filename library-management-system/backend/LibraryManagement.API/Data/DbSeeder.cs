using LibraryManagement.API.Models;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagement.API.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (!context.Users.Any(u => u.IsAdmin))
        {
            context.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = HashPassword("admin123"),
                IsAdmin = true
            });
            context.SaveChanges();
        }

        if (context.Books.Any()) return;

        var bookData = new[]
        {
            ("Animal Farm",                              "George Orwell",          "Political Satire",      "A short allegorical novel in which farm animals overthrow their human farmer, only to find that their pig leaders gradually become as corrupt and oppressive as the humans they replaced. A sharp critique of totalitarianism and political propaganda."),
            ("Brave New World",                          "Aldous Huxley",          "Dystopian Fiction",     "Set in a futuristic World State where citizens are manufactured through genetic engineering and conditioned from birth, this novel explores a society where happiness is enforced through consumerism, promiscuity, and the drug Soma — raising profound questions about freedom and humanity."),
            ("Crime and Punishment",                     "Fyodor Dostoevsky",      "Classic Fiction",       "A psychological thriller following Raskolnikov, a destitute student in St. Petersburg who murders a pawnbroker, convinced his intellectual superiority puts him above moral law. The novel traces his mental anguish, guilt, and eventual path toward redemption."),
            ("Don Quixote",                              "Miguel de Cervantes",    "Classic Fiction",       "Often called the first modern novel, this comic masterpiece follows Alonso Quixano, a man who reads so many chivalric romances that he loses his mind and sets out as Don Quixote, a self-styled knight, accompanied by the earthy squire Sancho Panza on a series of absurd misadventures."),
            ("Dune",                                     "Frank Herbert",          "Science Fiction",       "Set in the distant future on the desert planet Arrakis, the only source of the universe's most precious substance, Dune follows young Paul Atreides as his family takes control of the planet and is thrust into a brutal political and religious conflict. A landmark of science fiction."),
            ("Emma",                                     "Jane Austen",            "Classic Romance",       "Emma Woodhouse is a clever, wealthy young woman who fancies herself a matchmaker in the English village of Highbury. Her well-intentioned meddling in others' romantic affairs — and her own obliviousness to her feelings — make for one of Austen's most witty and beloved novels."),
            ("Frankenstein",                             "Mary Shelley",           "Gothic Horror",         "Victor Frankenstein, obsessed with the secret of life, creates a living creature from assembled body parts. When he abandons his hideous creation, the creature — intelligent, sensitive, and desperate for belonging — turns to violence. A landmark of Gothic literature and the birth of science fiction."),
            ("Gone with the Wind",                       "Margaret Mitchell",      "Historical Fiction",    "Set against the backdrop of the American Civil War and Reconstruction, this sweeping novel follows the tempestuous Scarlett O'Hara and her complex, passionate relationship with the roguish Rhett Butler, set against the destruction of the Old South."),
            ("Harry Potter and the Philosopher's Stone", "J.K. Rowling",           "Fantasy",               "Eleven-year-old Harry Potter discovers he is a wizard and has been accepted into Hogwarts School of Witchcraft and Wizardry. The novel launches one of the most beloved fantasy series in history, following Harry's first year navigating magic, friendship, and a dark hidden past."),
            ("It",                                       "Stephen King",           "Horror",                "In the town of Derry, Maine, a group of childhood friends known as the Losers' Club confronts a shapeshifting entity that most often takes the form of Pennywise the Dancing Clown. Decades later, the survivors must return to face their deepest fears once more."),
            ("Jane Eyre",                                "Charlotte Brontë",       "Classic Fiction",       "An orphaned girl of plain appearance but fierce spirit, Jane Eyre works her way from a cruel charity school to a position as governess at Thornfield Hall, where she falls deeply in love with the brooding, mysterious Mr. Rochester — only to uncover a terrible secret about him."),
            ("Kafka on the Shore",                       "Haruki Murakami",        "Magical Realism",       "Two parallel narratives intertwine: fifteen-year-old Kafka Tamura runs away from home to escape an Oedipal prophecy, while Nakata, an elderly man who lost his memory in the war, searches for a missing cat. Fish rain from the sky, time loops, and reality bends in Murakami's dreamlike masterwork."),
            ("Little Women",                             "Louisa May Alcott",      "Classic Fiction",       "Following the lives of the four March sisters — Meg, Jo, Beth, and Amy — as they grow from childhood to womanhood in Civil War-era New England, this beloved novel explores family, ambition, love, and loss with warmth and timeless insight."),
            ("Moby Dick",                                "Herman Melville",        "Adventure / Classic",   "Captain Ahab commands the whaling ship Pequod on a monomaniacal quest to hunt and kill Moby Dick — the enormous white sperm whale that bit off his leg. Narrated by the sailor Ishmael, this epic meditation on obsession, fate, and humanity remains one of the greatest American novels."),
            ("Nineteen Eighty-Four",                     "George Orwell",          "Dystopian Fiction",     "In the totalitarian super-state of Oceania, Winston Smith secretly rebels against the ruling Party led by the omnipresent Big Brother. This terrifying vision of a surveillance state — complete with Newspeak, doublethink, and the Thought Police — remains the defining dystopian novel of the modern era."),
            ("Of Mice and Men",                          "John Steinbeck",         "Classic Fiction",       "George and Lennie, two displaced migrant farm workers, dream of owning their own land during the Great Depression. Their tender, complex friendship is tested by poverty, prejudice, and Lennie's uncontrollable strength. A heartbreaking short novel about the nature of dreams and companionship."),
            ("Pride and Prejudice",                      "Jane Austen",            "Classic Romance",       "The witty Elizabeth Bennet and the proud Mr. Darcy clash, misunderstand each other, and slowly fall in love in Regency England. Austen's razor-sharp social commentary and unforgettable characters make this one of the most popular novels in the English language."),
            ("Sapiens: A Brief History of Humankind",    "Yuval Noah Harari",      "Non-Fiction / History", "Harari surveys the history of humankind from the Stone Age through the twenty-first century, exploring how biology and history shaped us. He examines the Cognitive, Agricultural, and Scientific Revolutions, and asks bold questions about what made Homo sapiens the dominant species on Earth."),
            ("The Alchemist",                            "Paulo Coelho",           "Philosophical Fiction", "Santiago, an Andalusian shepherd boy, dreams of a treasure hidden near the Egyptian pyramids. His journey across the Sahara becomes a metaphysical quest guided by the universe itself. Coelho's internationally beloved fable is a meditation on destiny, dreams, and the soul of the world."),
            ("To Kill a Mockingbird",                    "Harper Lee",             "Classic Fiction",       "Told through the eyes of young Scout Finch in Depression-era Alabama, this Pulitzer Prize-winning novel follows her father, lawyer Atticus Finch, as he defends a Black man falsely accused of raping a white woman. A profound exploration of racial injustice, moral courage, and the loss of innocence."),
            ("Ulysses",                                  "James Joyce",            "Modernist Fiction",     "Following Leopold Bloom and Stephen Dedalus through a single day in Dublin on 16 June 1904, Joyce's monumental novel parallels Homer's Odyssey while capturing the stream of consciousness of its characters in extraordinary detail. Considered one of the most important works of the twentieth century."),
            ("Wuthering Heights",                        "Emily Brontë",           "Gothic Romance",        "The foundling Heathcliff and his foster sister Catherine fall deeply in love on the Yorkshire moors, but class and cruelty tear them apart. Heathcliff's obsessive pursuit of revenge across two generations unfolds in this dark, passionate, and structurally intricate Gothic masterpiece."),
        };

        foreach (var (title, author, genre, description) in bookData)
        {
            var book = new Book
            {
                Title = title,
                Author = author,
                Genre = genre,
                Description = description,
                Copies = Enumerable.Range(1, 20)
                    .Select(n => new BookCopy { CopyNumber = n })
                    .ToList()
            };
            context.Books.Add(book);
        }

        context.SaveChanges();
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hash);
    }
}
