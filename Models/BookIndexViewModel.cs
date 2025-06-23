using BookCatalog.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

public class BookIndexViewModel
{
    public List<Book> Books { get; set; }
    public SelectList Genres { get; set; }
    public string BookGenre { get; set; }
    public string SearchString { get; set; }
}