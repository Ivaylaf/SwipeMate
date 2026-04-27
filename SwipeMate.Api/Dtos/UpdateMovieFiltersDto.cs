namespace SwipeMate.Api.Dtos;

public class UpdateMovieFiltersDto
{
    public List<string> Genres { get; set; } = new();
    public double? MinRating { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
}
