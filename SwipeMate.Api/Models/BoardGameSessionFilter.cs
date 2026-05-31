using System.ComponentModel.DataAnnotations;

namespace SwipeMate.Api.Models;

public class BoardGameSessionFilter
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public string UserId { get; set; } = default!;

    public string? GameType { get; set; }

    public int? DurationMin { get; set; }
    public int? DurationMax { get; set; }

    public int? PlayersMin { get; set; }
    public int? PlayersMax { get; set; }

    public double? MinRating { get; set; }
}

