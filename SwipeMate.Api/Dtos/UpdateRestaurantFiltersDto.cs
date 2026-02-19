namespace SwipeMate.Api.Dtos;

public class UpdateRestaurantFiltersDto
{
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Cuisine { get; set; }
    public double? MinRating { get; set; }
}

