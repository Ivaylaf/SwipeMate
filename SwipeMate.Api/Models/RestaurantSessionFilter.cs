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

    // Bulgaria-only (MVP): City + District
    public string? City { get; set; }
    public string? District { get; set; }

    // e.g. "Italian", "Bulgarian", "Asian"
    public string? Cuisine { get; set; }

    // 0..10
    public double? MinRating { get; set; }
}
