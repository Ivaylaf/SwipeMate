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
    private const string DataSnapshot = "2026-05-30";

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
        Movie("movie_tt0111161", "The Shawshank Redemption", "movie", 9.3, 1994, 142, ["Drama"],
            "Двама затворници изграждат приятелство и надежда в продължение на години.",
            "https://www.imdb.com/title/tt0111161/"),
        Movie("movie_tt0068646", "The Godfather", "movie", 9.2, 1972, 175, ["Crime", "Drama"],
            "Криминална драма за семейство Корлеоне и цената на властта.",
            "https://www.imdb.com/title/tt0068646/"),
        Movie("movie_tt0468569", "The Dark Knight", "movie", 9.0, 2008, 152, ["Action", "Crime", "Drama"],
            "Батман се изправя срещу Жокера в мрачен и напрегнат Готъм.",
            "https://www.imdb.com/title/tt0468569/"),
        Movie("movie_tt0816692", "Interstellar", "movie", 8.7, 2014, 169, ["Adventure", "Drama", "Sci-Fi"],
            "Научнофантастично пътешествие за оцеляването на човечеството.",
            "https://www.imdb.com/title/tt0816692/"),
        Movie("movie_tt1375666", "Inception", "movie", 8.8, 2010, 148, ["Action", "Adventure", "Sci-Fi"],
            "Трилър за сънища, подсъзнание и внимателно планиран обир.",
            "https://www.imdb.com/title/tt1375666/"),
        Movie("movie_tt0110912", "Pulp Fiction", "movie", 8.9, 1994, 154, ["Crime", "Drama"],
            "Нелинейна криминална история с култови персонажи и диалози.",
            "https://www.imdb.com/title/tt0110912/"),
        Movie("movie_tt6751668", "Parasite", "movie", 8.5, 2019, 132, ["Drama", "Thriller"],
            "Социален трилър за класи, измама и напрежение в едно семейство.",
            "https://www.imdb.com/title/tt6751668/"),
        Movie("movie_tt7286456", "Joker", "movie", 8.4, 2019, 122, ["Crime", "Drama", "Thriller"],
            "Психологическа история за изолация, гняв и превръщане в злодей.",
            "https://www.imdb.com/title/tt7286456/"),
        Movie("movie_tt8946378", "Knives Out", "movie", 7.9, 2019, 130, ["Comedy", "Crime", "Mystery"],
            "Стилен криминален пъзел с много обрати и черен хумор.",
            "https://www.imdb.com/title/tt8946378/"),
        Movie("movie_tt0120737", "The Lord of the Rings: The Fellowship of the Ring", "movie", 8.9, 2001, 178, ["Adventure", "Drama", "Fantasy"],
            "Началото на епично фентъзи пътешествие през Средната земя.",
            "https://www.imdb.com/title/tt0120737/"),
        Movie("movie_tt0903747", "Breaking Bad", "series", 9.5, 2008, 49, ["Crime", "Drama", "Thriller"],
            "Учител по химия постепенно навлиза в света на престъпността.",
            "https://www.imdb.com/title/tt0903747/"),
        Movie("movie_tt4574334", "Stranger Things", "series", 8.7, 2016, 51, ["Drama", "Fantasy", "Horror"],
            "Група приятели се сблъскват със свръхестествени събития в малък град.",
            "https://www.imdb.com/title/tt4574334/")
    ];

    private static readonly IReadOnlyList<DemoCatalogItem> Restaurants =
    [
        Restaurant("restaurant_pavaj_plovdiv", "Pavaj", "Plovdiv", "Kapana", "Bulgarian", ["Bulgarian", "Modern", "European"], 4.6, "$$$",
            "Популярен ресторант в Капана с модерен прочит на българската кухня.",
            "https://www.tripadvisor.com/Restaurant_Review-g295391-d7322856-Reviews-Pavaj-Plovdiv_Plovdiv_Province.html"),
        Restaurant("restaurant_rahat_tepe_plovdiv", "Rahat Tepe", "Plovdiv", "Old Town", "Bulgarian", ["Bulgarian", "BBQ", "Traditional"], 4.4, "$$",
            "Заведение в Стария град с традиционни ястия и панорамна гледка.",
            "https://www.tripadvisor.com/Restaurant_Review-g295391-d2309235-Reviews-Rahat_Tepe-Plovdiv_Plovdiv_Province.html"),
        Restaurant("restaurant_smolini_plovdiv", "Smokini", "Plovdiv", "Center", "European", ["European", "Modern", "Fusion"], 4.5, "$$$",
            "Модерно място в центъра с авторски ястия и елегантна атмосфера.",
            "https://www.tripadvisor.com/Restaurant_Review-g295391-d8074677-Reviews-Smokini-Plovdiv_Plovdiv_Province.html"),
        Restaurant("restaurant_hemingway_plovdiv", "Hemingway", "Plovdiv", "Center", "European", ["European", "Mediterranean"], 4.4, "$$",
            "Ресторант с европейска кухня, подходящ за вечеря с приятели.",
            "https://www.tripadvisor.com/Restaurant_Review-g295391-d1044807-Reviews-Hemingway-Plovdiv_Plovdiv_Province.html"),
        Restaurant("restaurant_torro_grande_plovdiv", "Torro Grande", "Plovdiv", "Center", "Steakhouse", ["Steakhouse", "European", "Pasta"], 4.3, "$$$",
            "Място за месни специалитети, паста и по-дълга вечеря.",
            "https://www.tripadvisor.com/Restaurant_Review-g295391-d7905182-Reviews-Torro_Grande-Plovdiv_Plovdiv_Province.html"),
        Restaurant("restaurant_happy_plovdiv", "Happy Bar & Grill", "Plovdiv", "Center", "International", ["International", "Sushi", "Burgers"], 4.2, "$$",
            "Разнообразно меню за групи с различни предпочитания.",
            "https://happy.bg/"),
        Restaurant("restaurant_jagerhof_plovdiv", "Jagerhof", "Plovdiv", "Trakia", "German", ["German", "Beerhouse", "European"], 4.5, "$$",
            "Бирен ресторант с немска кухня и просторна атмосфера.",
            "https://www.tripadvisor.com/Restaurant_Review-g295391-d21270719-Reviews-Jagerhof-Plovdiv_Plovdiv_Province.html"),
        Restaurant("restaurant_sasa_sofia", "Sasa Asian Pub", "Sofia", "Center", "Asian", ["Asian", "Japanese", "Sushi"], 4.4, "$$",
            "Азиатски вкусове, суши и коктейли за вечер с приятели.",
            "https://www.tripadvisor.com/Restaurant_Review-g294452-d8484124-Reviews-SASA_Asian_Pub-Sofia_Sofia_Region.html"),
        Restaurant("restaurant_made_in_home_sofia", "Made in Home", "Sofia", "Center", "Bulgarian", ["Bulgarian", "European", "Homemade"], 4.6, "$$",
            "Домашна кухня с уютна атмосфера и модерен стил.",
            "https://www.tripadvisor.com/Restaurant_Review-g294452-d3785288-Reviews-Made_in_Home-Sofia_Sofia_Region.html"),
        Restaurant("restaurant_shtastlivetsa_sofia", "Shtastlivetsa", "Sofia", "Vitosha Blvd", "Bulgarian", ["Bulgarian", "European", "Traditional"], 4.5, "$$",
            "Популярен ресторант с богато меню и традиционни български вкусове.",
            "https://www.tripadvisor.com/Restaurant_Review-g294452-d6956502-Reviews-Shtastlivetsa_Vitoshka-Sofia_Sofia_Region.html"),
        Restaurant("restaurant_cosmos_sofia", "Cosmos", "Sofia", "Center", "European", ["European", "Contemporary", "Bulgarian"], 4.7, "$$$",
            "Съвременна кухня и дегустационно изживяване в центъра на София.",
            "https://guide.michelin.com/bg/en/sofia-region/sofia/restaurant/cosmos"),
        Restaurant("restaurant_moma_sofia", "Moma", "Sofia", "Center", "Bulgarian", ["Bulgarian", "Traditional", "European"], 4.5, "$$$",
            "Изискани български вкусове в стилен интериор.",
            "https://www.tripadvisor.com/Restaurant_Review-g294452-d10632765-Reviews-Moma_Bulgarian_Food_and_Wine-Sofia_Sofia_Region.html"),
        Restaurant("restaurant_raketa_sofia", "Raketa Rakia Bar", "Sofia", "Lozenets", "Bulgarian", ["Bulgarian", "Eastern European", "Bar"], 4.4, "$$",
            "Атрактивно място с българска кухня и ретро атмосфера.",
            "https://www.tripadvisor.com/Restaurant_Review-g294452-d8435609-Reviews-Raketa_Rakia_Bar-Sofia_Sofia_Region.html"),
        Restaurant("restaurant_mr_baba_varna", "Mr Baba", "Varna", "Center", "Seafood", ["Seafood", "European", "Mediterranean"], 4.5, "$$",
            "Ресторант на кораб във Варна с морско меню и гледка.",
            "https://www.tripadvisor.com/Restaurant_Review-g295392-d3703381-Reviews-Mr_Baba-Varna_Varna_Province.html"),
        Restaurant("restaurant_staria_chinar_varna", "Staria Chinar", "Varna", "Center", "Bulgarian", ["Bulgarian", "European", "BBQ"], 4.4, "$$",
            "Познат варненски ресторант с българска кухня и месни специалитети.",
            "https://www.tripadvisor.com/Restaurant_Review-g295392-d4239547-Reviews-Staria_Chinar-Varna_Varna_Province.html"),
        Restaurant("restaurant_the_martini_varna", "The Martini Food & Cocktails", "Varna", "Sea Garden", "International", ["International", "Cocktails", "Fusion"], 4.4, "$$$",
            "Съвременно място край Морската градина за вечеря и коктейли.",
            "https://www.tripadvisor.com/Restaurant_Review-g295392-d8655624-Reviews-The_Martini_Food_Cocktails-Varna_Varna_Province.html"),
        Restaurant("restaurant_happy_varna", "Happy Bar & Grill Varna", "Varna", "Center", "International", ["International", "Sushi", "Burgers"], 4.2, "$$",
            "Разнообразно меню и удобно място за голяма компания.",
            "https://happy.bg/"),
        Restaurant("restaurant_di_jamie_burgas", "Di Jamie", "Burgas", "Center", "Italian", ["Italian", "Pizza", "Mediterranean"], 4.5, "$$",
            "Италианска кухня и пица в центъра на Бургас.",
            "https://www.tripadvisor.com/Restaurant_Review-g303653-d12139167-Reviews-Di_Jamie-Burgas_Burgas_Province.html"),
        Restaurant("restaurant_ti_bar_burgas", "Ti Bar & Kitchen", "Burgas", "Center", "European", ["European", "Seafood", "Modern"], 4.4, "$$$",
            "Модерно меню и градска атмосфера за вечерно излизане.",
            "https://www.tripadvisor.com/Restaurant_Review-g303653-d12240425-Reviews-Ti_Bar_Kitchen-Burgas_Burgas_Province.html"),
        Restaurant("restaurant_neptune_burgas", "Neptune", "Burgas", "Center", "Seafood", ["Seafood", "European", "Mediterranean"], 4.3, "$$$",
            "Ресторант с морски специалитети и централна локация в Бургас.",
            "https://www.tripadvisor.com/Restaurant_Review-g303653-d2251570-Reviews-Neptune-Burgas_Burgas_Province.html")
    ];
    private static readonly IReadOnlyList<DemoCatalogItem> Recipes =
    [
        Recipe("recipe_52977", "Corba", "Turkish", "Soup", 2, 1, 4.4, 40, ["lentils", "onion", "carrots", "spices"],
            "Топла супа с леща, подходяща за лека вечеря.",
            "https://www.themealdb.com/meal/52977"),
        Recipe("recipe_52893", "Apple & Blackberry Crumble", "British", "Dessert", 2, 2, 4.6, 50, ["apples", "blackberries", "flour", "sugar"],
            "Плодов десерт с хрупкава коричка.",
            "https://www.themealdb.com/meal/52893"),
        Recipe("recipe_52772", "Teriyaki Chicken Casserole", "Japanese", "Dinner", 3, 2, 4.7, 60, ["chicken", "rice", "soy sauce", "vegetables"],
            "Пилешко ястие с ориз и терияки вкус.",
            "https://www.themealdb.com/meal/52772"),
        Recipe("recipe_52844", "Lasagne", "Italian", "Dinner", 4, 3, 4.8, 90, ["beef", "pasta", "tomato", "cheese"],
            "Класическа италианска лазаня за споделена вечеря.",
            "https://www.themealdb.com/meal/52844"),
        Recipe("recipe_52959", "Burek", "Balkan", "Breakfast", 3, 1, 4.6, 70, ["filo pastry", "cheese", "egg", "oil"],
            "Балканска закуска с кори и сирене.",
            "https://www.themealdb.com/meal/52959"),
        Recipe("recipe_52795", "Chicken Handi", "Indian", "Dinner", 3, 2, 4.7, 45, ["chicken", "tomato", "cream", "spices"],
            "Индийско пилешко ястие с богат сос.",
            "https://www.themealdb.com/meal/52795"),
        Recipe("recipe_52855", "Banana Pancakes", "American", "Breakfast", 1, 1, 4.5, 20, ["banana", "eggs", "flour", "milk"],
            "Бърза сладка закуска с банани.",
            "https://www.themealdb.com/meal/52855"),
        Recipe("recipe_52906", "Flamiche", "French", "Lunch", 3, 2, 4.3, 55, ["leek", "pastry", "cream", "cheese"],
            "Френски солен пай с праз и кремообразна плънка.",
            "https://www.themealdb.com/meal/52906"),
        Recipe("recipe_52948", "Wontons", "Chinese", "Dinner", 4, 2, 4.6, 65, ["pork", "wonton wrappers", "ginger", "soy sauce"],
            "Китайски пълнени хапки, подходящи за по-специална вечер.",
            "https://www.themealdb.com/meal/52948"),
        Recipe("recipe_53013", "Big Mac", "American", "Dinner", 2, 2, 4.2, 35, ["beef", "bun", "lettuce", "cheese"],
            "Домашна версия на познат бургер.",
            "https://www.themealdb.com/meal/53013")
    ];

    private static readonly IReadOnlyList<DemoCatalogItem> BoardGames =
    [
        BoardGame("game_13", "Catan", "Strategy", 60, 120, 3, 4, 7.1, 3,
            "Стратегическа игра за ресурси, строене и преговори.",
            "https://boardgamegeek.com/boardgame/13/catan"),
        BoardGame("game_178900", "Codenames", "Party", 15, 30, 2, 8, 7.5, 2,
            "Отборна игра с думи, асоциации и внимателни подсказки.",
            "https://boardgamegeek.com/boardgame/178900/codenames"),
        BoardGame("game_9209", "Ticket to Ride", "Family", 30, 60, 2, 5, 7.4, 2,
            "Семейна игра за влакови маршрути и планиране.",
            "https://boardgamegeek.com/boardgame/9209/ticket-to-ride"),
        BoardGame("game_39856", "Dixit", "Creative", 30, 45, 3, 8, 7.2, 1,
            "Креативна игра с изображения, истории и асоциации.",
            "https://boardgamegeek.com/boardgame/39856/dixit"),
        BoardGame("game_167791", "Terraforming Mars", "Strategy", 120, 180, 1, 5, 8.3, 5,
            "Дълга стратегическа игра за развитие на Марс.",
            "https://boardgamegeek.com/boardgame/167791/terraforming-mars"),
        BoardGame("game_230802", "Azul", "Abstract", 30, 45, 2, 4, 7.7, 2,
            "Красива абстрактна игра с плочки и тактическо мислене.",
            "https://boardgamegeek.com/boardgame/230802/azul"),
        BoardGame("game_266192", "Wingspan", "Strategy", 40, 70, 1, 5, 8.0, 3,
            "Игра за птици, карти и изграждане на двигател.",
            "https://boardgamegeek.com/boardgame/266192/wingspan"),
        BoardGame("game_148228", "Splendor", "Strategy", 30, 45, 2, 4, 7.4, 2,
            "Игра с карти, ресурси и плавно надграждане.",
            "https://boardgamegeek.com/boardgame/148228/splendor"),
        BoardGame("game_822", "Carcassonne", "Tile Placement", 30, 45, 2, 5, 7.4, 2,
            "Класическа игра с плочки, градове, пътища и ферми.",
            "https://boardgamegeek.com/boardgame/822/carcassonne"),
        BoardGame("game_169786", "Scythe", "Strategy", 90, 115, 1, 5, 8.1, 4,
            "Стратегическа игра с контрол на територии и развитие.",
            "https://boardgamegeek.com/boardgame/169786/scythe")
    ];

    private static DemoCatalogItem Movie(string id, string title, string kind, double rating, int year, int duration, string[] genres, string description, string sourceUrl) =>
        new(
            id,
            title,
            "https://images.unsplash.com/photo-1489599849927-2ee91cede3ba?q=80&w=1200&auto=format&fit=crop",
            new
            {
                kind,
                genres,
                rating,
                year,
                duration,
                description,
                sourceName = "IMDb",
                sourceUrl,
                dataSnapshot = DataSnapshot
            });

    private static DemoCatalogItem Restaurant(string id, string title, string city, string district, string cuisine, string[] cuisines, double rating, string priceRange, string description, string sourceUrl) =>
        new(
            id,
            title,
            "https://images.unsplash.com/photo-1414235077428-338989a2e8c0?q=80&w=1200&auto=format&fit=crop",
            new
            {
                city,
                district,
                cuisine,
                cuisines,
                rating,
                priceRange,
                description,
                sourceName = "Tripadvisor / official website snapshot",
                sourceUrl,
                dataSnapshot = DataSnapshot
            });

    private static DemoCatalogItem Recipe(string id, string title, string cuisine, string foodType, int complexity, int budgetLevel, double rating, int prepTime, string[] ingredients, string description, string sourceUrl) =>
        new(
            id,
            title,
            "https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?q=80&w=1200&auto=format&fit=crop",
            new
            {
                complexity,
                cuisine,
                foodType,
                budgetLevel,
                rating,
                prepTime,
                ingredients,
                description,
                sourceName = "TheMealDB",
                sourceUrl,
                dataSnapshot = DataSnapshot
            });

    private static DemoCatalogItem BoardGame(string id, string title, string gameType, int durationMin, int durationMax, int playersMin, int playersMax, double rating, int complexity, string description, string sourceUrl) =>
        new(
            id,
            title,
            "https://images.unsplash.com/photo-1629760946220-5693ee4c46ac?q=80&w=1200&auto=format&fit=crop",
            new
            {
                gameType,
                durationMin,
                durationMax,
                playersMin,
                playersMax,
                rating,
                complexity,
                description,
                sourceName = "BoardGameGeek",
                sourceUrl,
                dataSnapshot = DataSnapshot
            });
}


