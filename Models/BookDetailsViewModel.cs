namespace BookCatalog.Models;

public class BookDetailsViewModel
{
    public Book Book { get; set; }
    public List<Book> StaffPicks { get; set; } = new();
}