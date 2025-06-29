using System.ComponentModel.DataAnnotations;

namespace GraphQLApi.Models;

public class Book
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int AuthorId { get; set; } // Foreign key identifier to Author in separate database

    public DateTime PublishedDate { get; set; }

    [Required]
    public decimal Price { get; set; }

    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}