namespace BookCatalog.Models;

using System.ComponentModel.DataAnnotations;

public class Book
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = "";

    [Required]
    public string Author { get; set; } = "";

    [Required]
    public string Genre { get; set; } = "";

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative number.")]
    public int Quantity { get; set; }

    [Required]
    [Range(0, 999.99, ErrorMessage = "Price must be a non-negative number.")]
    public decimal Price { get; set; }

    public string? CoverImageUrl { get; set; }
    public string? CoverImagePath { get; set; }

    [Range(0.0, 5.0, ErrorMessage = "Rating must be between 0 and 5.")]
    public double? Rating { get; set; }

    public string? ISBN { get; set; }

    public bool IsStaffPick { get; set; }
}