using SwipeMate.Mobile.Services;

namespace SwipeMate.Mobile.Pages;

public partial class HomePage : ContentPage
{
    private readonly AppState _appState;
    private readonly SwipeMateApiService _apiService;
    private bool _isCompactLayout;

    public HomePage(AppState appState, SwipeMateApiService apiService)
    {
        InitializeComponent();
        _appState = appState;
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        WelcomeLabel.Text = $"Добре дошъл, {_appState.User?.UserName ?? "приятелю"}!";
        AdminBadge.IsVisible = _appState.User?.IsAdmin == true;
        AdminPanelCard.IsVisible = _appState.User?.IsAdmin == true;
        UpdateResponsiveLayout(Width);

        try
        {
            var requests = await _apiService.GetFriendRequestsAsync();
            RequestsBadge.IsVisible = requests.Count > 0;
            RequestsBadgeLabel.Text = requests.Count.ToString();
        }
        catch
        {
            RequestsBadge.IsVisible = false;
        }
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        UpdateResponsiveLayout(width);
    }

    private void UpdateResponsiveLayout(double width)
    {
        var shouldUseCompactLayout = width is > 0 and < 620;
        if (_isCompactLayout == shouldUseCompactLayout)
        {
            return;
        }

        _isCompactLayout = shouldUseCompactLayout;

        if (shouldUseCompactLayout)
        {
            WelcomeLabel.FontSize = 24;
            SubtitleLabel.FontSize = 14;
            CategoryGrid.ColumnSpacing = 12;
            CategoryGrid.RowSpacing = 12;
            CategoryGrid.RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition(new GridLength(164)),
                new RowDefinition(new GridLength(164))
            };

            ApplyCategoryCompact(MoviesLabel, MoviesIcon);
            ApplyCategoryCompact(RestaurantsLabel, RestaurantsIcon);
            ApplyCategoryCompact(RecipesLabel, RecipesIcon);
            ApplyCategoryCompact(GamesLabel, GamesIcon);

            QuickActionsGrid.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            };
            QuickActionsGrid.RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            };

            ApplyActionCompact(FriendsActionGrid, FriendsActionLabel, FriendsActionIcon);
            ApplyActionCompact(ActiveSessionsActionGrid, ActiveSessionsActionLabel, ActiveSessionsActionIcon);
            ApplyActionCompact(HistoryActionGrid, HistoryActionLabel, HistoryActionIcon);
            ApplyActionCompact(ProfileActionGrid, ProfileActionLabel, ProfileActionIcon);

            Grid.SetColumn(FriendsActionCard, 0);
            Grid.SetRow(FriendsActionCard, 0);
            Grid.SetColumn(ActiveSessionsActionCard, 1);
            Grid.SetRow(ActiveSessionsActionCard, 0);
            Grid.SetColumn(HistoryActionCard, 0);
            Grid.SetRow(HistoryActionCard, 1);
            Grid.SetColumn(ProfileActionCard, 1);
            Grid.SetRow(ProfileActionCard, 1);
            return;
        }

        WelcomeLabel.FontSize = 31;
        SubtitleLabel.FontSize = 16;
        CategoryGrid.ColumnSpacing = 18;
        CategoryGrid.RowSpacing = 18;
        CategoryGrid.RowDefinitions = new RowDefinitionCollection
        {
            new RowDefinition(new GridLength(192)),
            new RowDefinition(new GridLength(192))
        };

        ApplyCategoryRegular(MoviesLabel, MoviesIcon);
        ApplyCategoryRegular(RestaurantsLabel, RestaurantsIcon);
        ApplyCategoryRegular(RecipesLabel, RecipesIcon);
        ApplyCategoryRegular(GamesLabel, GamesIcon);

        QuickActionsGrid.ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition(GridLength.Star),
            new ColumnDefinition(GridLength.Star),
            new ColumnDefinition(GridLength.Star),
            new ColumnDefinition(GridLength.Star)
        };
        QuickActionsGrid.RowDefinitions = new RowDefinitionCollection
        {
            new RowDefinition(GridLength.Auto)
        };

        ApplyActionRegular(FriendsActionGrid, FriendsActionLabel, FriendsActionIcon);
        ApplyActionRegular(ActiveSessionsActionGrid, ActiveSessionsActionLabel, ActiveSessionsActionIcon);
        ApplyActionRegular(HistoryActionGrid, HistoryActionLabel, HistoryActionIcon);
        ApplyActionRegular(ProfileActionGrid, ProfileActionLabel, ProfileActionIcon);

        Grid.SetColumn(FriendsActionCard, 0);
        Grid.SetRow(FriendsActionCard, 0);
        Grid.SetColumn(ActiveSessionsActionCard, 1);
        Grid.SetRow(ActiveSessionsActionCard, 0);
        Grid.SetColumn(HistoryActionCard, 2);
        Grid.SetRow(HistoryActionCard, 0);
        Grid.SetColumn(ProfileActionCard, 3);
        Grid.SetRow(ProfileActionCard, 0);
    }

    private static void ApplyCategoryCompact(Label label, VisualElement icon)
    {
        label.FontSize = 18;
        label.MaxLines = 2;
        icon.WidthRequest = 42;
        icon.HeightRequest = 42;
    }

    private static void ApplyCategoryRegular(Label label, VisualElement icon)
    {
        label.FontSize = 24;
        label.MaxLines = 2;
        icon.WidthRequest = 54;
        icon.HeightRequest = 54;
    }

    private static void ApplyActionCompact(Grid grid, Label label, VisualElement icon)
    {
        grid.HeightRequest = 84;
        label.FontSize = 13;
        label.MaxLines = 2;
        icon.WidthRequest = 18;
        icon.HeightRequest = 18;
    }

    private static void ApplyActionRegular(Grid grid, Label label, VisualElement icon)
    {
        grid.HeightRequest = 94;
        label.FontSize = 14;
        label.MaxLines = 2;
        icon.WidthRequest = 20;
        icon.HeightRequest = 20;
    }

    private Task NavigateToCategoryAsync(string category)
        => Shell.Current.GoToAsync($"{nameof(CreateSessionPage)}?category={Uri.EscapeDataString(category)}");

    private async void OnMoviesClicked(object sender, EventArgs e) => await NavigateToCategoryAsync("Movie");
    private async void OnRestaurantsClicked(object sender, EventArgs e) => await NavigateToCategoryAsync("Restaurant");
    private async void OnRecipesClicked(object sender, EventArgs e) => await NavigateToCategoryAsync("Recipe");
    private async void OnGamesClicked(object sender, EventArgs e) => await NavigateToCategoryAsync("BoardGame");
    private async void OnFriendsClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(FriendsPage));
    private async void OnHistoryClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(HistoryPage));
    private async void OnProfileClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(ProfilePage));
    private async void OnActiveSessionsClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(ActiveSessionsPage));
    private async void OnAdminClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(AdminPage));

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await _appState.LogoutAsync();
        await Shell.Current.GoToAsync("//Login");
    }
}


