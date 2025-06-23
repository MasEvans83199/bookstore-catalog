using BookCatalog.Models;

namespace BookCatalog.Data;

using Microsoft.EntityFrameworkCore;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        context.Database.Migrate();
        
        if (context.Books.Any())
            return;

        var books = new Book[]
        {
            new Book
            {
                Title = "The Great Gatsby",
                Author = "F. Scott Fitzgerald",
                Genre = "Classic",
                Quantity = 5,
                Price = 10.99m,
                CoverImageUrl = "https://m.media-amazon.com/images/I/81AFhc5pVgL._AC_UF1000,1000_QL80_.jpg"
            },
            new Book
            {
                Title = "1984",
                Author = "George Orwell",
                Genre = "Dystopian",
                Quantity = 8,
                Price = 9.99m,
                CoverImageUrl = "https://m.media-amazon.com/images/I/71kxa1-0mfL._AC_UF1000,1000_QL80_.jpg"
            },
            new Book
            {
                Title = "To Kill a Mockingbird",
                Author = "Harper Lee",
                Genre = "Fiction",
                Quantity = 7,
                Price = 12.50m,
                CoverImageUrl = "https://m.media-amazon.com/images/I/81gepf1eMqL._AC_UF1000,1000_QL80_.jpg"
            },
            new Book
            {
                Title = "Brave New World",
                Author = "Aldous Huxley",
                Genre = "Dystopian",
                Quantity = 6,
                Price = 11.49m,
                CoverImageUrl = "https://m.media-amazon.com/images/I/71-gNR3fDCL._AC_UF1000,1000_QL80_.jpg"
            },
            new Book
            {
                Title = "The Catcher in the Rye",
                Author = "J.D. Salinger",
                Genre = "Classic",
                Quantity = 4,
                Price = 8.99m,
                CoverImageUrl = "https://m.media-amazon.com/images/I/81OthjkJBuL._AC_UF1000,1000_QL80_.jpg"
            },
            new Book
            {
                Title = "Pride and Prejudice",
                Author = "Jane Austen",
                Genre = "Romance",
                Quantity = 10,
                Price = 7.99m,
                CoverImageUrl = "https://m.media-amazon.com/images/I/81pW0H90J+L._AC_UF1000,1000_QL80_.jpg"
            },
            new Book
            {
                Title = "The Hobbit",
                Author = "J.R.R. Tolkien",
                Genre = "Fantasy",
                Quantity = 9,
                Price = 13.95m,
                CoverImageUrl = "https://m.media-amazon.com/images/I/91b0C2YNSrL._AC_UF1000,1000_QL80_.jpg"
            },
            new Book
            {
                Title = "The Fellowship of the Ring",
                Author = "J.R.R. Tolkien",
                Genre = "Fantasy",
                Quantity = 5,
                Price = 14.99m,
                CoverImageUrl = "https://m.media-amazon.com/images/I/91pR9wKJ3zL._AC_UF1000,1000_QL80_.jpg"
            },
            new Book
            {
                Title = "Frankenstein",
                Author = "Mary Shelley",
                Genre = "Gothic",
                Quantity = 6,
                Price = 6.75m,
                CoverImageUrl = "https://m.media-amazon.com/images/I/81iqZ2HHD-L._AC_UF1000,1000_QL80_.jpg"
            },
            new Book
            {
                Title = "The Picture of Dorian Gray",
                Author = "Oscar Wilde",
                Genre = "Philosophical",
                Quantity = 3,
                Price = 9.50m,
                CoverImageUrl = "https://m.media-amazon.com/images/I/71M3dpdK9-L._AC_UF1000,1000_QL80_.jpg"
            }
            
        };

        context.Books.AddRange(books);
        context.SaveChanges();
    }
}