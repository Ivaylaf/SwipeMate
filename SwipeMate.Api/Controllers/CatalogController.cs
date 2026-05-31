using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwipeMate.Api.Data;
using System.Text.Json;

namespace SwipeMate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CatalogController : ControllerBase
{
    private readonly AppDbContext _db;

    public CatalogController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions()
    {
        var items = await _db.CatalogItems
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync();

        var movies = items.Where(x => x.Category == "Movie").Select(ParseMeta).ToList();
        var restaurants = items.Where(x => x.Category == "Restaurant").Select(ParseMeta).ToList();
        var recipes = items.Where(x => x.Category == "Recipe").Select(ParseMeta).ToList();
        var boardGames = items.Where(x => x.Category == "BoardGame").Select(ParseMeta).ToList();

        return Ok(new
        {
            movies = new
            {
                genres = DistinctStrings(movies.SelectMany(x => GetStrings(x, "genres"))),
                yearMin = MinInt(movies, "year"),
                yearMax = MaxInt(movies, "year")
            },
            restaurants = new
            {
                cities = DistinctStrings(restaurants.Select(x => GetString(x, "city"))),
                districts = DistinctStrings(restaurants.Select(x => GetString(x, "district"))),
                districtsByCity = restaurants
                    .Select(x => new { City = GetString(x, "city"), District = GetString(x, "district") })
                    .Where(x => !string.IsNullOrWhiteSpace(x.City) && !string.IsNullOrWhiteSpace(x.District))
                    .GroupBy(x => x.City!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.District!).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList(),
                        StringComparer.OrdinalIgnoreCase),
                cuisines = DistinctStrings(
                    restaurants.Select(x => GetString(x, "cuisine"))
                        .Concat(restaurants.SelectMany(x => GetStrings(x, "cuisines"))))
            },
            recipes = new
            {
                cuisines = DistinctStrings(recipes.Select(x => GetString(x, "cuisine"))),
                foodTypes = DistinctStrings(recipes.Select(x => GetString(x, "foodType"))),
                ingredients = DistinctStrings(recipes.SelectMany(x => GetStrings(x, "ingredients"))),
                complexityMin = MinInt(recipes, "complexity"),
                complexityMax = MaxInt(recipes, "complexity"),
                budgetMin = MinInt(recipes, "budgetLevel"),
                budgetMax = MaxInt(recipes, "budgetLevel")
            },
            boardGames = new
            {
                gameTypes = DistinctStrings(boardGames.Select(x => GetString(x, "gameType"))),
                playersMin = MinInt(boardGames, "playersMin"),
                playersMax = MaxInt(boardGames, "playersMax"),
                durationMin = MinInt(boardGames, "durationMin"),
                durationMax = MaxInt(boardGames, "durationMax")
            }
        });
    }

    private static JsonElement ParseMeta(Models.CatalogItem item)
        => string.IsNullOrWhiteSpace(item.MetaJson)
            ? default
            : JsonSerializer.Deserialize<JsonElement>(item.MetaJson);

    private static List<string> DistinctStrings(IEnumerable<string?> values)
        => values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

    private static string? GetString(JsonElement meta, string property)
        => meta.ValueKind == JsonValueKind.Object && meta.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static List<string> GetStrings(JsonElement meta, string property)
    {
        if (meta.ValueKind != JsonValueKind.Object || !meta.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return value.EnumerateArray()
            .Where(x => x.ValueKind == JsonValueKind.String)
            .Select(x => x.GetString())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .ToList();
    }

    private static int MinInt(IEnumerable<JsonElement> rows, string property)
        => rows.Select(x => GetInt(x, property)).Where(x => x > 0).DefaultIfEmpty().Min();

    private static int MaxInt(IEnumerable<JsonElement> rows, string property)
        => rows.Select(x => GetInt(x, property)).Where(x => x > 0).DefaultIfEmpty().Max();

    private static int GetInt(JsonElement meta, string property)
        => meta.ValueKind == JsonValueKind.Object && meta.TryGetProperty(property, out var value) && value.TryGetInt32(out var number)
            ? number
            : 0;
}


