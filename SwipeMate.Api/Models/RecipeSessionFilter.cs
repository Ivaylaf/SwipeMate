using System.ComponentModel.DataAnnotations;

namespace SwipeMate.Api.Models;

public class RecipeSessionFilter
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public string UserId { get; set; } = default!;

    public int? Complexity { get; set; }
    public string? Cuisine { get; set; }
    public string? FoodType { get; set; }
    public int? BudgetLevel { get; set; }
    public double? MinRating { get; set; }

    public string? IngredientsCsv { get; set; }
}

