namespace SwipeMate.Api.Dtos;

public class BoardGameFiltersDto
{
    public string? GameType { get; set; }

    public int? DurationMin { get; set; }
    public int? DurationMax { get; set; }

    public int? PlayersMin { get; set; }
    public int? PlayersMax { get; set; }

    public double? MinRating { get; set; }
}

