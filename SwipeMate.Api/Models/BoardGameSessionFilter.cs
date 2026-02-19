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

    // Тип игра (може да е само 1 избор за MVP)
    public string? GameType { get; set; } // Strategy, Party, Cooperative...

    // Продължителност в минути (диапазон)
    public int? DurationMin { get; set; }
    public int? DurationMax { get; set; }

    // Брой играчи (диапазон)
    public int? PlayersMin { get; set; }
    public int? PlayersMax { get; set; }

    // Оценка
    public double? MinRating { get; set; }
}
