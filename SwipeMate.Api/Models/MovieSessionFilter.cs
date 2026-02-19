using System.ComponentModel.DataAnnotations;

namespace SwipeMate.Api.Models;

public class MovieSessionFilter
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public string UserId { get; set; } = default!;

    public double? MinRating { get; set; }       // 0..10
    public int? YearFrom { get; set; }           // e.g. 1990
    public int? YearTo { get; set; }             // e.g. 2024

    // Genres: "Action,Comedy" (лесно за MVP)
    public string? GenresCsv { get; set; }
}

