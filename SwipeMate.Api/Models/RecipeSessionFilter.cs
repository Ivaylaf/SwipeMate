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

    // Filters
    public int? Complexity { get; set; }          // 1..5 (пример)
    public string? Cuisine { get; set; }          // "Bulgarian", "Italian", "Vegetarian"...
    public string? FoodType { get; set; }         // "Main", "Dessert", "Starter"...
    public int? BudgetLevel { get; set; }         // 1..5 (пример)
    public double? MinRating { get; set; }        // 0..10 (или 0..5)

    // MVP: продукти като CSV, напр: "chicken,tomato,cheese"
    public string? IngredientsCsv { get; set; }
}
