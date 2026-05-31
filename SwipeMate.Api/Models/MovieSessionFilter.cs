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

    public double? MinRating { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }

    public string? GenresCsv { get; set; }
}


