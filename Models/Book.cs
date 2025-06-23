namespace BookCatalog.Models;

using System.ComponentModel.DataAnnotations;

public class Book
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; }

    public string Author { get; set; }

    public string Genre { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    public string CoverImageUrl { get; set; }
}