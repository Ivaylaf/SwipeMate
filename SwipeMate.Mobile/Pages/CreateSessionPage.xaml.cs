using SwipeMate.Mobile.Models;
using SwipeMate.Mobile.Services;

namespace SwipeMate.Mobile.Pages;

[QueryProperty(nameof(Category), "category")]
[QueryProperty(nameof(SessionId), "sessionId")]
public partial class CreateSessionPage : ContentPage
{
    private readonly SwipeMateApiService _apiService;
    private readonly AppState _appState;
    private readonly HashSet<string> _selectedFriends = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _selectedMovieGenres = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _selectedRecipeIngredients = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _selectedRecipeCuisines = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _selectedGameTypes = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, List<string>> _restaurantDistrictsByCity = new(StringComparer.OrdinalIgnoreCase);
    private bool _catalogLoaded;
    private string _category = "Movie";
    private Guid? _sessionId;

    public CreateSessionPage(SwipeMateApiService apiService, AppState appState)
    {
        InitializeComponent();
        _apiService = apiService;
        _appState = appState;
        ApplyCategoryVisuals();
        ConfigurePageMode();
    }

    public string Category
    {
        get => _category;
        set
        {
            _category = string.IsNullOrWhiteSpace(value) ? "Movie" : Uri.UnescapeDataString(value);
            ApplyCategoryVisuals();
        }
    }

    public string SessionId
    {
        get => _sessionId?.ToString() ?? string.Empty;
        set
        {
            if (Guid.TryParse(Uri.UnescapeDataString(value ?? string.Empty), out var parsed))
            {
                _sessionId = parsed;
            }
            else
            {
                _sessionId = null;
            }

            ConfigurePageMode();
            ApplyCategoryVisuals();
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await EnsureCatalogOptionsAsync();

        if (_sessionId.HasValue)
        {
            ContinueButton.IsEnabled = true;
            NoFriendsHintLabel.IsVisible = false;
            return;
        }

        await LoadFriendsAsync();
    }

    private void ConfigurePageMode()
    {
        if (PageTitleLabel is null || ContinueButton is null || FriendsCard is null)
        {
            return;
        }

        var isContributionMode = _sessionId.HasValue;
        PageTitleLabel.Text = isContributionMode ? "Филтри за сесията" : "Създай сесия за съвпадение";
        ContinueButton.Text = isContributionMode ? "Запази моите филтри" : "Изпрати покани";
        FriendsCard.IsVisible = !isContributionMode;
        ContinueButton.IsEnabled = true;
    }

    private async Task EnsureCatalogOptionsAsync()
    {
        if (_catalogLoaded)
        {
            return;
        }

        try
        {
            var options = await _apiService.GetCatalogOptionsAsync();
            ApplyCatalogOptions(options);
            _catalogLoaded = true;
        }
        catch
        {
            ApplyFallbackCatalogOptions();
            _catalogLoaded = true;
        }
    }

    private void ApplyCatalogOptions(CatalogOptionsSummary options)
    {
        PopulateCheckOptions(MovieGenresFlexLayout, options.Movies.Genres, _selectedMovieGenres, OnMovieGenreCheckedChanged);
        PopulateCheckOptions(RecipeIngredientsFlexLayout, options.Recipes.Ingredients, _selectedRecipeIngredients, OnRecipeIngredientCheckedChanged);

        _restaurantDistrictsByCity = options.Restaurants.DistrictsByCity
            .ToDictionary(x => x.Key, x => x.Value.OrderBy(v => v).ToList(), StringComparer.OrdinalIgnoreCase);

        RestaurantCityPicker.ItemsSource = WithAll(options.Restaurants.Cities);
        RestaurantDistrictPicker.ItemsSource = WithAll(options.Restaurants.Districts);
        RestaurantCuisinePicker.ItemsSource = WithAll(options.Restaurants.Cuisines);

        PopulateCheckOptions(RecipeCuisinesFlexLayout, options.Recipes.Cuisines, _selectedRecipeCuisines, OnRecipeCuisineCheckedChanged);
        RecipeFoodTypePicker.ItemsSource = WithAll(options.Recipes.FoodTypes);

        PopulateCheckOptions(GameTypesFlexLayout, options.BoardGames.GameTypes, _selectedGameTypes, OnGameTypeCheckedChanged);

        ApplyMovieYearRange(options.Movies.YearMin, options.Movies.YearMax);
        ApplyBoardGameRanges(options.BoardGames);
        ApplyRecipeRanges(options.Recipes);

        SelectFirstPickerItem(RestaurantCityPicker);
        ApplyRestaurantDistrictOptions();
        SelectFirstPickerItem(RestaurantCuisinePicker);        SelectFirstPickerItem(RecipeFoodTypePicker);

    }

    private void ApplyFallbackCatalogOptions()
    {
        ApplyCatalogOptions(new CatalogOptionsSummary
        {
            Movies = new MovieCatalogOptions
            {
                Genres = ["Action", "Comedy", "Drama", "Sci-Fi", "Horror", "Romance", "Thriller"],
                YearMin = 1970,
                YearMax = 2024
            },
            Restaurants = new RestaurantCatalogOptions
            {
                Cities = ["Plovdiv"],
                Districts = ["Center", "Kapana", "Trakia", "Smirnenski"],
                Cuisines = ["Bulgarian", "European", "Healthy", "Italian", "Japanese"]
            },
            Recipes = new RecipeCatalogOptions
            {
                Cuisines = ["American", "Bulgarian", "Italian", "Japanese", "Mexican"],
                FoodTypes = ["Breakfast", "Dinner", "Salad"],
                Ingredients = ["chicken", "egg", "mushrooms", "pasta", "tomatoes"],
                ComplexityMin = 1,
                ComplexityMax = 5,
                BudgetMin = 1,
                BudgetMax = 3
            },
            BoardGames = new BoardGameCatalogOptions
            {
                GameTypes = ["Creative", "Family", "Party", "Strategy"],
                PlayersMin = 1,
                PlayersMax = 8,
                DurationMin = 15,
                DurationMax = 180
            }
        });
    }

    private async Task LoadFriendsAsync()
    {
        try
        {
            var friends = await _apiService.GetFriendsAsync();
            FriendsCollectionView.ItemsSource = friends;
            NoFriendsHintLabel.IsVisible = friends.Count == 0;
            ContinueButton.IsEnabled = true;
            ConfigurePageMode();
            UpdateSelectedFriendsLabel();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Грешка", ex.Message, "OK");
        }
    }

    private void ApplyCategoryVisuals()
    {
        if (CategoryTitleLabel is null)
        {
            return;
        }

        var settings = Category switch
        {
            "Restaurant" => new CategoryVisuals("Ресторанти", "#F97316", "#DC2626"),
            "Recipe" => new CategoryVisuals("Рецепти", "#22C55E", "#059669"),
            "BoardGame" => new CategoryVisuals("Настолни игри", "#8B5CF6", "#EC4899"),
            _ => new CategoryVisuals("Филми и сериали", "#0EA5E9", "#A855F7")
        };

        CategoryTitleLabel.Text = settings.Title;
        CategorySubtitleLabel.Text = _sessionId.HasValue ? "Добави своите филтри преди гласуването" : "Избери приятели за общото решение";
        CategoryGradientStart.Color = Color.FromArgb(settings.GradientStart);
        CategoryGradientEnd.Color = Color.FromArgb(settings.GradientEnd);
        MovieCategoryIconPath.IsVisible = Category == "Movie";
        RestaurantCategoryIconPath.IsVisible = Category == "Restaurant";
        RecipeCategoryIconPath.IsVisible = Category == "Recipe";
        BoardGameCategoryIconPath.IsVisible = Category == "BoardGame";

        MovieFiltersLayout.IsVisible = Category == "Movie";
        RestaurantFiltersLayout.IsVisible = Category == "Restaurant";
        RecipeFiltersLayout.IsVisible = Category == "Recipe";
        GameFiltersLayout.IsVisible = Category == "BoardGame";
    }

    private void PopulateCheckOptions(FlexLayout layout, IEnumerable<string> values, HashSet<string> selected, EventHandler<CheckedChangedEventArgs> handler)
    {
        layout.Children.Clear();
        selected.Clear();

        foreach (var value in values.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x))
        {
            var checkBox = new CheckBox
            {
                BindingContext = value,
                VerticalOptions = LayoutOptions.Center
            };
            checkBox.CheckedChanged += handler;

            var label = new Label
            {
                Text = value,
                TextColor = Color.FromArgb("#111827"),
                VerticalTextAlignment = TextAlignment.Center
            };

            var stack = new HorizontalStackLayout
            {
                Spacing = 4,
                WidthRequest = 132,
                Margin = new Thickness(0, 0, 8, 8),
                Children = { checkBox, label }
            };

            layout.Children.Add(stack);
        }
    }

    private static List<string> WithAll(IEnumerable<string> values)
        => ["Всички", .. values.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x)];

    private static void SelectFirstPickerItem(Picker picker)
    {
        if (picker.ItemsSource is IList<string> values && values.Count > 0)
        {
            picker.SelectedIndex = 0;
        }
    }

    private void ApplyMovieYearRange(int minYear, int maxYear)
    {
        var from = minYear > 0 ? minYear : 1970;
        var to = maxYear >= from ? maxYear : 2024;

        MovieYearFromSlider.Minimum = from;
        MovieYearFromSlider.Maximum = to;
        MovieYearFromSlider.Value = Math.Max(from, Math.Min(1990, to));

        MovieYearToSlider.Minimum = from;
        MovieYearToSlider.Maximum = to;
        MovieYearToSlider.Value = to;

        UpdateMovieYearLabel();
    }

    private void ApplyBoardGameRanges(BoardGameCatalogOptions options)
    {
        var minPlayers = options.PlayersMin > 0 ? options.PlayersMin : 1;
        var maxPlayers = options.PlayersMax >= minPlayers ? options.PlayersMax : 8;
        var minDuration = options.DurationMin > 0 ? options.DurationMin : 15;
        var maxDuration = options.DurationMax >= minDuration ? options.DurationMax : 180;

        GamePlayersMinSlider.Minimum = minPlayers;
        GamePlayersMinSlider.Maximum = maxPlayers;
        GamePlayersMinSlider.Value = minPlayers;

        GamePlayersMaxSlider.Minimum = minPlayers;
        GamePlayersMaxSlider.Maximum = maxPlayers;
        GamePlayersMaxSlider.Value = maxPlayers;

        GameDurationMinSlider.Minimum = minDuration;
        GameDurationMinSlider.Maximum = maxDuration;
        GameDurationMinSlider.Value = minDuration;

        GameDurationMaxSlider.Minimum = minDuration;
        GameDurationMaxSlider.Maximum = maxDuration;
        GameDurationMaxSlider.Value = maxDuration;

        GamePlayersLabel.Text = $"Играчи: {minPlayers} - {maxPlayers}";
        GameDurationLabel.Text = $"Продължителност: {minDuration} - {maxDuration} мин";
    }

    private void ApplyRecipeRanges(RecipeCatalogOptions options)
    {
        var minComplexity = options.ComplexityMin > 0 ? options.ComplexityMin : 1;
        var maxComplexity = options.ComplexityMax >= minComplexity ? options.ComplexityMax : 5;

        RecipeComplexitySlider.Minimum = minComplexity;
        RecipeComplexitySlider.Maximum = maxComplexity;
        RecipeComplexitySlider.Value = maxComplexity;
        RecipeComplexityLabel.Text = $"Максимална сложност: {maxComplexity}";

    }

    private void UpdateSelectedFriendsLabel()
    {
        SelectedFriendsLabel.Text = $"Избрани приятели ({_selectedFriends.Count})";
        ContinueButton.IsEnabled = true;
    }

    private void OnFriendCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is not CheckBox checkBox || checkBox.BindingContext is not FriendSummary friend)
        {
            return;
        }

        if (e.Value)
        {
            _selectedFriends.Add(friend.UserName);
        }
        else
        {
            _selectedFriends.Remove(friend.UserName);
        }

        UpdateSelectedFriendsLabel();
    }

    private void OnRestaurantCityChanged(object? sender, EventArgs e)
    {
        ApplyRestaurantDistrictOptions();
    }

    private void ApplyRestaurantDistrictOptions()
    {
        var selectedCity = PickerValue(RestaurantCityPicker);
        var districts = string.IsNullOrWhiteSpace(selectedCity)
            ? _restaurantDistrictsByCity.Values.SelectMany(x => x).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList()
            : _restaurantDistrictsByCity.TryGetValue(selectedCity, out var cityDistricts)
                ? cityDistricts
                : new List<string>();

        RestaurantDistrictPicker.ItemsSource = WithAll(districts);
        SelectFirstPickerItem(RestaurantDistrictPicker);
    }

    private void OnMovieGenreCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is not CheckBox checkBox || checkBox.BindingContext is not string genre)
        {
            return;
        }

        if (e.Value)
        {
            _selectedMovieGenres.Add(genre);
        }
        else
        {
            _selectedMovieGenres.Remove(genre);
        }
    }

    private void OnRecipeIngredientCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is not CheckBox checkBox || checkBox.BindingContext is not string ingredient)
        {
            return;
        }

        if (e.Value)
        {
            _selectedRecipeIngredients.Add(ingredient);
        }
        else
        {
            _selectedRecipeIngredients.Remove(ingredient);
        }
    }


    private void OnRecipeCuisineCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is not CheckBox checkBox || checkBox.BindingContext is not string cuisine)
        {
            return;
        }

        if (e.Value)
        {
            _selectedRecipeCuisines.Add(cuisine);
        }
        else
        {
            _selectedRecipeCuisines.Remove(cuisine);
        }
    }

    private void OnGameTypeCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is not CheckBox checkBox || checkBox.BindingContext is not string gameType)
        {
            return;
        }

        if (e.Value)
        {
            _selectedGameTypes.Add(gameType);
        }
        else
        {
            _selectedGameTypes.Remove(gameType);
        }
    }
    private async void OnStartClicked(object sender, EventArgs e)
    {
        if (_sessionId.HasValue)
        {
            try
            {
                await SaveFiltersAsync(_sessionId.Value);
                if (!await EnsureSessionHasSuggestionsAsync(_sessionId.Value, closeIfEmpty: false))
                {
                    return;
                }
                await DisplayAlert("Запазено", "Филтрите ти бяха добавени към сесията. Обединените предложения вече ще използват запазените предпочитания на всички участници.", "OK");
                await Shell.Current.GoToAsync(nameof(ActiveSessionsPage));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Филтрите не можаха да бъдат запазени", ex.Message, "OK");
            }

            return;
        }

        if (_selectedFriends.Count == 0)
        {
            await DisplayAlert("Избери приятел", "Избери поне един приятел, преди да изпратиш поканите за сесия.", "OK");
            return;
        }

        try
        {
            var session = await _apiService.CreateSessionAsync(Category, _selectedFriends);
            await SaveFiltersAsync(session.SessionId);
            if (!await EnsureSessionHasSuggestionsAsync(session.SessionId, closeIfEmpty: true))
            {
                return;
            }

            _appState.CurrentSessionId = session.SessionId;
            _appState.CurrentCategory = session.Category;
            _appState.CurrentMatch = null;

            await DisplayAlert("Поканите са изпратени", "Сесията е създадена и поканите бяха изпратени до избраните приятели. Всеки участник вече може да добави собствените си филтри преди гласуването.", "OK");
            await Shell.Current.GoToAsync(nameof(ActiveSessionsPage));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Сесията не можа да бъде създадена", ex.Message, "OK");
        }
    }

    private async Task<bool> EnsureSessionHasSuggestionsAsync(Guid sessionId, bool closeIfEmpty)
    {
        var availableCount = await _apiService.GetAvailableSuggestionCountAsync(sessionId);
        if (availableCount > 0)
        {
            return true;
        }

        if (closeIfEmpty)
        {
            try
            {
                await _apiService.CloseSessionAsync(sessionId);
            }
            catch
            {
                // Cleanup is best-effort; the important part is to stop the user before swiping.
            }
        }

        await DisplayAlert("Няма предложения", "Избраните филтри не намират подходящи предложения. Промени филтрите и опитай отново.", "OK");
        return false;
    }
    private Task SaveFiltersAsync(Guid sessionId)
    {
        return Category switch
        {
            "Restaurant" => _apiService.SaveRestaurantFiltersAsync(sessionId,
                PickerValue(RestaurantCityPicker),
                PickerValue(RestaurantDistrictPicker),
                PickerValue(RestaurantCuisinePicker),
                SliderValue(RestaurantMinRatingSlider, "0.0")),
            "Recipe" => _apiService.SaveRecipeFiltersAsync(sessionId,
                SliderValue(RecipeComplexitySlider, "0"),
                string.Join(",", _selectedRecipeCuisines.OrderBy(x => x)),
                PickerValue(RecipeFoodTypePicker),
                SliderValue(RecipeMinRatingSlider, "0.0"),
                string.Join(",", _selectedRecipeIngredients.OrderBy(x => x))),
            "BoardGame" => _apiService.SaveBoardGameFiltersAsync(sessionId,
                string.Join(",", _selectedGameTypes.OrderBy(x => x)),
                SliderValue(GameDurationMinSlider, "0"),
                SliderValue(GameDurationMaxSlider, "0"),
                SliderValue(GamePlayersMinSlider, "0"),
                SliderValue(GamePlayersMaxSlider, "0"),
                SliderValue(GameMinRatingSlider, "0.0")),
            _ => _apiService.SaveMovieFiltersAsync(sessionId,
                string.Join(",", _selectedMovieGenres.OrderBy(x => x)),
                SliderValue(MovieMinRatingSlider, "0.0"),
                SliderValue(MovieYearFromSlider, "0"),
                SliderValue(MovieYearToSlider, "0"))
        };
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("..");

    private static string PickerValue(Picker picker)
    {
        var value = picker.SelectedItem?.ToString();
        return string.IsNullOrWhiteSpace(value) || value == "Всички" ? "" : value;
    }

    private static string SliderValue(Slider slider, string format)
        => Math.Round(slider.Value).ToString(format);

    private void OnMovieRatingValueChanged(object sender, ValueChangedEventArgs e)
        => MovieMinRatingLabel.Text = $"Минимална оценка: {e.NewValue:0.0}+";

    private void OnMovieRatingChanged(object sender, EventArgs e)
        => MovieMinRatingSlider.Value = Math.Round(MovieMinRatingSlider.Value * 2) / 2;

    private void OnMovieYearValueChanged(object sender, ValueChangedEventArgs e)
        => UpdateMovieYearLabel();

    private void OnMovieYearChanged(object sender, EventArgs e)
    {
        MovieYearFromSlider.Value = Math.Round(MovieYearFromSlider.Value);
        MovieYearToSlider.Value = Math.Round(MovieYearToSlider.Value);
        if (MovieYearFromSlider.Value > MovieYearToSlider.Value)
        {
            MovieYearToSlider.Value = MovieYearFromSlider.Value;
        }

        UpdateMovieYearLabel();
    }

    private void UpdateMovieYearLabel()
        => MovieYearLabel.Text = $"Година на издаване: {(int)Math.Round(MovieYearFromSlider.Value)} - {(int)Math.Round(MovieYearToSlider.Value)}";

    private void OnRestaurantRatingValueChanged(object sender, ValueChangedEventArgs e)
        => RestaurantMinRatingLabel.Text = $"Минимална оценка: {Math.Round(e.NewValue * 2) / 2:0.0}+";

    private void OnRecipeComplexityValueChanged(object sender, ValueChangedEventArgs e)
        => RecipeComplexityLabel.Text = $"Максимална сложност: {(int)Math.Round(e.NewValue)}";


    private void OnRecipeRatingValueChanged(object sender, ValueChangedEventArgs e)
        => RecipeMinRatingLabel.Text = $"Минимална оценка: {Math.Round(e.NewValue * 2) / 2:0.0}+";

    private void OnGamePlayersValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (GamePlayersMinSlider.Value > GamePlayersMaxSlider.Value)
        {
            GamePlayersMaxSlider.Value = GamePlayersMinSlider.Value;
        }

        GamePlayersLabel.Text = $"Играчи: {(int)Math.Round(GamePlayersMinSlider.Value)} - {(int)Math.Round(GamePlayersMaxSlider.Value)}";
    }

    private void OnGameDurationValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (GameDurationMinSlider.Value > GameDurationMaxSlider.Value)
        {
            GameDurationMaxSlider.Value = GameDurationMinSlider.Value;
        }

        GameDurationLabel.Text = $"Продължителност: {(int)Math.Round(GameDurationMinSlider.Value)} - {(int)Math.Round(GameDurationMaxSlider.Value)} мин";
    }

    private void OnGameRatingValueChanged(object sender, ValueChangedEventArgs e)
        => GameMinRatingLabel.Text = $"Минимална оценка: {Math.Round(e.NewValue * 2) / 2:0.0}+";

    private sealed record CategoryVisuals(string Title, string GradientStart, string GradientEnd);
}










