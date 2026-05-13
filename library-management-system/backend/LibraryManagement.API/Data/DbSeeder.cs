using LibraryManagement.API.Models;

namespace LibraryManagement.API.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Books.Any()) return;

        var books = new List<Book>
        {
            new() { Title = "Pakistan", IsAvailable = true, CountryOverview = "Pakistan is a South Asian country with a population of over 220 million. It is home to the ancient Indus Valley Civilization, the mighty Karakoram mountain range including K2, and a rich culture of art, music, and cuisine. Islamabad is the capital city." },
            new() { Title = "United States", IsAvailable = true, CountryOverview = "The United States is a North American nation consisting of 50 states. It is the world's largest economy and known for its cultural diversity, technological innovation, Hollywood, and democratic traditions. Washington D.C. is the capital." },
            new() { Title = "United Kingdom", IsAvailable = true, CountryOverview = "The United Kingdom comprises England, Scotland, Wales, and Northern Ireland. It has a rich history including the British Empire, the Industrial Revolution, and iconic landmarks like Big Ben and Buckingham Palace. London is the capital." },
            new() { Title = "UAE", IsAvailable = true, CountryOverview = "The United Arab Emirates is a federation of seven emirates in the Arabian Peninsula. Known for its futuristic skyline, luxury tourism, and oil wealth, it is home to the world's tallest building, the Burj Khalifa. Abu Dhabi is the capital." },
            new() { Title = "China", IsAvailable = true, CountryOverview = "China is the world's most populous country with over 1.4 billion people. It is one of the world's oldest civilizations, home to the Great Wall, the Forbidden City, and a rapidly growing economy that is now the second largest globally. Beijing is the capital." },
            new() { Title = "France", IsAvailable = true, CountryOverview = "France is a Western European nation known for its art, fashion, cuisine, and culture. It is home to the Eiffel Tower, the Louvre Museum, and some of the world's finest wines. Paris, the capital, is often called the City of Light." },
            new() { Title = "Spain", IsAvailable = true, CountryOverview = "Spain is located on the Iberian Peninsula in Southern Europe. It is famous for its vibrant culture, flamenco dancing, and architectural wonders like the Sagrada Familia and the Alhambra Palace. Madrid is the capital and largest city." },
            new() { Title = "Russia", IsAvailable = true, CountryOverview = "Russia is the world's largest country by land area, spanning across Eastern Europe and Northern Asia. It has a rich history including the Soviet era, the Bolshoi Ballet, and stunning architecture like the Kremlin and St. Basil's Cathedral. Moscow is the capital." },
            new() { Title = "Mexico", IsAvailable = true, CountryOverview = "Mexico is a North American country with a rich indigenous heritage from the Aztec and Maya civilizations. It is famous for its diverse cuisine, vibrant festivals, ancient pyramids like Chichen Itza, and beautiful beaches. Mexico City is the capital." },
            new() { Title = "Indonesia", IsAvailable = true, CountryOverview = "Indonesia is the world's largest archipelago nation, consisting of over 17,000 islands. It is the world's fourth most populous country and home to extraordinary biodiversity, including orangutans and Komodo dragons. Jakarta is the capital." },
        };

        context.Books.AddRange(books);
        context.SaveChanges();
    }
}
