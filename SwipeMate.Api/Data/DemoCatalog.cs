using System.Text.Json;
using SwipeMate.Api.Models;

namespace SwipeMate.Api.Data;

public sealed record DemoCatalogItem(
    string ExternalId,
    string Title,
    string? ImageUrl,
    object Meta);

public static class DemoCatalog
{
    public static IReadOnlyList<CatalogItem> CreateCatalogItems()
    {
        return new[] { "Movie", "Restaurant", "Recipe", "BoardGame" }
            .SelectMany(category => GetItems(category).Select(item => new CatalogItem
            {
                Category = category,
                ExternalId = item.ExternalId,
                Title = item.Title,
                ImageUrl = item.ImageUrl,
                MetaJson = JsonSerializer.Serialize(item.Meta),
                IsActive = true
            }))
            .ToList();
    }

    public static IReadOnlyList<SessionItem> CreateSessionItems(Guid sessionId, string category)
    {
        var normalized = NormalizeCategory(category);

        return GetItems(normalized)
            .Select(item => new SessionItem
            {
                SessionId = sessionId,
                Category = normalized,
                ExternalId = item.ExternalId,
                Title = item.Title,
                ImageUrl = item.ImageUrl,
                MetaJson = JsonSerializer.Serialize(item.Meta)
            })
            .ToList();
    }

    public static string NormalizeCategory(string? category)
    {
        return category?.Trim().ToLowerInvariant() switch
        {
            "movie" or "movies" => "Movie",
            "restaurant" or "restaurants" => "Restaurant",
            "recipe" or "recipes" => "Recipe",
            "boardgame" or "boardgames" or "game" or "games" => "BoardGame",
            _ => "Movie"
        };
    }

    private static IReadOnlyList<DemoCatalogItem> GetItems(string category) => category switch
    {
        "Restaurant" => Restaurants,
        "Recipe" => Recipes,
        "BoardGame" => BoardGames,
        _ => Movies
    };

    private static readonly IReadOnlyList<DemoCatalogItem> Movies =
    [
        new(
            "movie_interstellar",
            "Interstellar",
            "https://images.unsplash.com/photo-1536440136628-849c177e76a1?q=80&w=1200&auto=format&fit=crop",
            new
            {
                kind = "movie",
                genres = new[] { "Sci-Fi", "Drama", "Adventure" },
                rating = 8.7,
                year = 2014,
                duration = 169,
                description = "Епично sci-fi пътешествие за оцеляването на човечеството."
            }),
        new(
            "movie_knives_out",
            "Knives Out",
            "https://images.unsplash.com/photo-1489599849927-2ee91cede3ba?q=80&w=1200&auto=format&fit=crop",
            new
            {
                kind = "movie",
                genres = new[] { "Mystery", "Comedy", "Crime" },
                rating = 7.9,
                year = 2019,
                duration = 130,
                description = "Стилен и забавен криминален пъзел с много обрати."
            }),
        new(
            "movie_stranger_things",
            "Stranger Things",
            "https://images.unsplash.com/photo-1509347528160-9a9e33742cdb?q=80&w=1200&auto=format&fit=crop",
            new
            {
                kind = "series",
                genres = new[] { "Fantasy", "Horror", "Sci-Fi" },
                rating = 8.6,
                year = 2016,
                duration = 50,
                description = "Носталгичен сериал с свръхестествени събития и силна приятелска динамика."
            }),
        new(
            "movie_lalaland",
            "La La Land",
            "https://images.unsplash.com/photo-1513106580091-1d82408b8cd6?q=80&w=1200&auto=format&fit=crop",
            new
            {
                kind = "movie",
                genres = new[] { "Romance", "Drama", "Music" },
                rating = 8.0,
                year = 2016,
                duration = 128,
                description = "Цветен, емоционален и музикален филм за мечти и избори."
            }),
        new(
            "movie_dark",
            "Dark",
            "https://images.unsplash.com/photo-1440404653325-ab127d49abc1?q=80&w=1200&auto=format&fit=crop",
            new
            {
                kind = "series",
                genres = new[] { "Thriller", "Mystery", "Sci-Fi" },
                rating = 8.7,
                year = 2017,
                duration = 55,
                description = "Интелигентен сериал с тайни, време и семейни мистерии."
            })
    ];

    private static readonly IReadOnlyList<DemoCatalogItem> Restaurants =
    [
        new(
            "restaurant_cosmos",
            "Cosmos Bistro",
            "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?q=80&w=1200&auto=format&fit=crop",
            new
            {
                city = "Plovdiv",
                district = "Center",
                cuisine = "European",
                cuisines = new[] { "European", "Bistro" },
                rating = 4.7,
                priceRange = "$$",
                description = "Модерно бистро в центъра с уютна атмосфера."
            }),
        new(
            "restaurant_aylyak_grill",
            "Aylyak Grill",
            "https://images.unsplash.com/photo-1552566626-52f8b828add9?q=80&w=1200&auto=format&fit=crop",
            new
            {
                city = "Plovdiv",
                district = "Kapana",
                cuisine = "Bulgarian",
                cuisines = new[] { "Bulgarian", "BBQ" },
                rating = 4.6,
                priceRange = "$$",
                description = "Българска кухня, скара и настроение за компания."
            }),
        new(
            "restaurant_sakura",
            "Sakura House",
            "https://images.unsplash.com/photo-1579027989536-b7b1f875659b?q=80&w=1200&auto=format&fit=crop",
            new
            {
                city = "Plovdiv",
                district = "Trakia",
                cuisine = "Japanese",
                cuisines = new[] { "Japanese", "Asian" },
                rating = 4.5,
                priceRange = "$$$",
                description = "Суши и азиатски специалитети за по-специална вечер."
            }),
        new(
            "restaurant_la_pasta",
            "La Pasta Fresca",
            "https://images.unsplash.com/photo-1555396273-367ea4eb4db5?q=80&w=1200&auto=format&fit=crop",
            new
            {
                city = "Plovdiv",
                district = "Smirnenski",
                cuisine = "Italian",
                cuisines = new[] { "Italian", "Pasta" },
                rating = 4.4,
                priceRange = "$$",
                description = "Прясна паста и уютна обстановка за приятелска вечеря."
            }),
        new(
            "restaurant_green_garden",
            "Green Garden",
            "https://images.unsplash.com/photo-1514933651103-005eec06c04b?q=80&w=1200&auto=format&fit=crop",
            new
            {
                city = "Plovdiv",
                district = "Center",
                cuisine = "Healthy",
                cuisines = new[] { "Healthy", "Vegan" },
                rating = 4.3,
                priceRange = "$$",
                description = "Леки и здравословни ястия, подходящи и за обяд."
            })
    ];

    private static readonly IReadOnlyList<DemoCatalogItem> Recipes =
    [
        new(
            "recipe_pasta_alfredo",
            "Creamy Alfredo Pasta",
            "https://images.unsplash.com/photo-1621996346565-e3dbc353d2e5?q=80&w=1200&auto=format&fit=crop",
            new
            {
                complexity = 2,
                cuisine = "Italian",
                foodType = "Dinner",
                budgetLevel = 2,
                rating = 4.7,
                prepTime = 25,
                ingredients = new[] { "pasta", "cream", "parmesan", "garlic" },
                description = "Бърза кремообразна паста за вечеря след училище или работа."
            }),
        new(
            "recipe_shopska_salad",
            "Shopska Salad",
            "https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?q=80&w=1200&auto=format&fit=crop",
            new
            {
                complexity = 1,
                cuisine = "Bulgarian",
                foodType = "Salad",
                budgetLevel = 1,
                rating = 4.8,
                prepTime = 10,
                ingredients = new[] { "tomatoes", "cucumbers", "peppers", "sirene" },
                description = "Класическа българска салата, лесна и винаги добра идея."
            }),
        new(
            "recipe_tacos",
            "Chicken Tacos",
            "https://images.unsplash.com/photo-1552332386-f8dd00dc2f85?q=80&w=1200&auto=format&fit=crop",
            new
            {
                complexity = 2,
                cuisine = "Mexican",
                foodType = "Dinner",
                budgetLevel = 2,
                rating = 4.5,
                prepTime = 30,
                ingredients = new[] { "chicken", "tortillas", "corn", "avocado" },
                description = "Свежи и засищащи tacos за споделена вечер."
            }),
        new(
            "recipe_pancakes",
            "Berry Pancakes",
            "https://images.unsplash.com/photo-1528207776546-365bb710ee93?q=80&w=1200&auto=format&fit=crop",
            new
            {
                complexity = 1,
                cuisine = "American",
                foodType = "Breakfast",
                budgetLevel = 1,
                rating = 4.6,
                prepTime = 20,
                ingredients = new[] { "flour", "milk", "eggs", "berries" },
                description = "Лесна рецепта за сладка закуска през уикенда."
            }),
        new(
            "recipe_ramen",
            "Homemade Ramen",
            "https://images.unsplash.com/photo-1617093727343-374698b1b08d?q=80&w=1200&auto=format&fit=crop",
            new
            {
                complexity = 4,
                cuisine = "Japanese",
                foodType = "Dinner",
                budgetLevel = 3,
                rating = 4.9,
                prepTime = 55,
                ingredients = new[] { "noodles", "broth", "egg", "mushrooms" },
                description = "По-амбициозна рецепта за ден, в който ви се готви нещо специално."
            })
    ];

    private static readonly IReadOnlyList<DemoCatalogItem> BoardGames =
    [
        new(
            "game_catan",
            "Catan",
            "https://images.unsplash.com/photo-1606503153255-59d8b8b25150?q=80&w=1200&auto=format&fit=crop",
            new
            {
                gameType = "Strategy",
                durationMin = 60,
                durationMax = 120,
                playersMin = 3,
                playersMax = 4,
                rating = 4.8,
                complexity = 3,
                description = "Класическа стратегическа игра за строене, ресурси и сделки."
            }),
        new(
            "game_codenames",
            "Codenames",
            "https://images.unsplash.com/photo-1529699211952-734e80c4d42b?q=80&w=1200&auto=format&fit=crop",
            new
            {
                gameType = "Party",
                durationMin = 15,
                durationMax = 30,
                playersMin = 4,
                playersMax = 8,
                rating = 4.7,
                complexity = 2,
                description = "Бърза отборна игра с думи, идеална за повече хора."
            }),
        new(
            "game_ticket_to_ride",
            "Ticket to Ride",
            "https://images.unsplash.com/photo-1511512578047-dfb367046420?q=80&w=1200&auto=format&fit=crop",
            new
            {
                gameType = "Family",
                durationMin = 45,
                durationMax = 75,
                playersMin = 2,
                playersMax = 5,
                rating = 4.6,
                complexity = 2,
                description = "Лека и приятна игра за маршрути и планиране."
            }),
        new(
            "game_dixit",
            "Dixit",
            "https://images.unsplash.com/photo-1553481187-be93c21490a9?q=80&w=1200&auto=format&fit=crop",
            new
            {
                gameType = "Creative",
                durationMin = 30,
                durationMax = 45,
                playersMin = 3,
                playersMax = 6,
                rating = 4.5,
                complexity = 1,
                description = "Креативна и красива игра с асоциации и въображение."
            }),
        new(
            "game_terraforming_mars",
            "Terraforming Mars",
            "https://images.unsplash.com/photo-1542751110-97427bbecf20?q=80&w=1200&auto=format&fit=crop",
            new
            {
                gameType = "Strategy",
                durationMin = 120,
                durationMax = 180,
                playersMin = 1,
                playersMax = 5,
                rating = 4.9,
                complexity = 5,
                description = "Дълга стратегическа игра за по-запалени геймъри."
            })
    ];
}
