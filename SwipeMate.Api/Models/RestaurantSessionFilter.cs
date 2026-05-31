using System.ComponentModel.DataAnnotations;

namespace SwipeMate.Api.Models;

public class RestaurantSessionFilter
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public string UserId { get; set; } = default!;

    public string? City { get; set; }
    public string? District { get; set; }

    public string? Cuisine { get; set; }

    public double? MinRating { get; set; }
}

