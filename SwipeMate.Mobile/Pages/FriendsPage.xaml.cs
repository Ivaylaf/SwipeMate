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
            FriendsHeaderLabel.Text = $"My Friends ({friends.Count})";
            RequestsHeaderLabel.Text = $"Requests ({requests.Count})";
            NoFriendsLabel.IsVisible = friends.Count == 0;
            NoRequestsLabel.IsVisible = requests.Count == 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
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
            NoSuggestionsLabel.Text = "Start typing to find existing users.";
            return;
        }

        try
        {
            var suggestions = await _apiService.SearchUsersAsync(text, ct);
            SuggestionsCollectionView.ItemsSource = suggestions;
            SuggestionsCollectionView.IsVisible = suggestions.Count > 0;
            NoSuggestionsLabel.Text = suggestions.Count == 0
                ? "No matching users found."
                : "Choose an existing user to send a request.";
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
            await DisplayAlert("Missing username", "Choose an existing user first.", "OK");
            return;
        }

        try
        {
            await _apiService.SendFriendRequestAsync(userName);
            FriendUserNameEntry.Text = string.Empty;
            SuggestionsCollectionView.ItemsSource = null;
            SuggestionsCollectionView.IsVisible = false;
            NoSuggestionsLabel.Text = "Friend request was sent.";
            await DisplayAlert("Sent", "Friend request was sent.", "OK");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Could not add friend", ex.Message, "OK");
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
            await DisplayAlert("Error", ex.Message, "OK");
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
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("..");
}
