namespace SwipeMate.Api.Dtos;

public class RecipeFiltersDto
{
    public int? Complexity { get; set; }
    public string? Cuisine { get; set; }
    public string? FoodType { get; set; }
    public int? BudgetLevel { get; set; }
    public double? MinRating { get; set; }

    public List<string> Ingredients { get; set; } = new();
}


