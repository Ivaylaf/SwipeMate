using SwipeMate.Mobile.Models;
using SwipeMate.Mobile.Services;

namespace SwipeMate.Mobile.Pages;

public partial class FriendsPage : ContentPage
{
    private readonly SwipeMateApiService _apiService;
    private CancellationTokenSource? _searchCts;

    public FriendsPage(SwipeMateApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
        await RefreshSuggestionsAsync(FriendUserNameEntry.Text);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var friends = await _apiService.GetFriendsAsync();
            var requests = await _apiService.GetFriendRequestsAsync();

            FriendsCollectionView.ItemsSource = friends;
            RequestsCollectionView.ItemsSource = requests;
            FriendsHeaderLabel.Text = $"Моите приятели ({friends.Count})";
            RequestsHeaderLabel.Text = $"Покани ({requests.Count})";
            NoFriendsLabel.IsVisible = friends.Count == 0;
            NoRequestsLabel.IsVisible = requests.Count == 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Грешка", ex.Message, "OK");
        }
    }

    private async void OnFriendSearchChanged(object sender, TextChangedEventArgs e)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        try
        {
            await Task.Delay(220, token);
            await RefreshSuggestionsAsync(e.NewTextValue, token);
        }
        catch (TaskCanceledException)
        {
        }
    }

    private async Task RefreshSuggestionsAsync(string? query, CancellationToken ct = default)
    {
        var text = query?.Trim() ?? string.Empty;
        if (text.Length < 2)
        {
            SuggestionsCollectionView.ItemsSource = null;
            SuggestionsCollectionView.IsVisible = false;
            NoSuggestionsLabel.Text = "Започни да пишеш, за да откриеш съществуващи потребители.";
            return;
        }

        try
        {
            var suggestions = await _apiService.SearchUsersAsync(text, ct);
            SuggestionsCollectionView.ItemsSource = suggestions;
            SuggestionsCollectionView.IsVisible = suggestions.Count > 0;
            NoSuggestionsLabel.Text = suggestions.Count == 0
                ? "Не бяха открити съвпадащи потребители."
                : "Избери съществуващ потребител, за да изпратиш покана.";
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            SuggestionsCollectionView.ItemsSource = null;
            SuggestionsCollectionView.IsVisible = false;
            NoSuggestionsLabel.Text = ex.Message;
        }
    }

    private async void OnSuggestionAddClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not FriendSummary friend)
        {
            return;
        }

        await SendFriendRequestAsync(friend.UserName);
    }

    private async Task SendFriendRequestAsync(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            await DisplayAlert("Липсва потребител", "Първо избери съществуващ потребител.", "OK");
            return;
        }

        try
        {
            await _apiService.SendFriendRequestAsync(userName);
            FriendUserNameEntry.Text = string.Empty;
            SuggestionsCollectionView.ItemsSource = null;
            SuggestionsCollectionView.IsVisible = false;
            NoSuggestionsLabel.Text = "Поканата за приятелство беше изпратена.";
            await DisplayAlert("Изпратено", "Поканата за приятелство беше изпратена.", "OK");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Неуспешно добавяне на приятел", ex.Message, "OK");
        }
    }

    private async void OnAcceptClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not FriendRequestSummary request)
        {
            return;
        }

        try
        {
            await _apiService.RespondToRequestAsync(request.Id, true);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Грешка", ex.Message, "OK");
        }
    }

    private async void OnRejectClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not FriendRequestSummary request)
        {
            return;
        }

        try
        {
            await _apiService.RespondToRequestAsync(request.Id, false);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Грешка", ex.Message, "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("..");
}
