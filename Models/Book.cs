namespace BookCatalog.Models;

using System.ComponentModel.DataAnnotations;

public class Book
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; }

    [Required]
    public string Author { get; set; }

    [Required]
    public string Genre { get; set; }
    [Display(Name = "ISBN")]
    public string? ISBN { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be 0 or more")]
    public int Quantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Price must be between $0.01 and $1000.00")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Url(ErrorMessage = "Must be a valid image URL")]
    public string CoverImageUrl { get; set; }
    public string CoverImagePath { get; set; }

    
    [Range(0.0, 5.0, ErrorMessage = "Rating must be between 0 and 5")]
    public double? Rating { get; set; } 
    
    public bool IsStaffPick { get; set; }
}