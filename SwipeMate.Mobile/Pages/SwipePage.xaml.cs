using System.Text.Json;
using SwipeMate.Mobile.Models;
using SwipeMate.Mobile.Services;

namespace SwipeMate.Mobile.Pages;

public partial class SwipePage : ContentPage
{
    private readonly SwipeMateApiService _apiService;
    private readonly AppState _appState;
    private SessionItemSummary? _currentItem;
    private int _swipeCount;

    public SwipePage(SwipeMateApiService apiService, AppState appState)
    {
        InitializeComponent();
        _apiService = apiService;
        _appState = appState;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        SwipeTitleLabel.Text = _appState.CurrentCategory ?? "Swipe";
        await LoadNextItemAsync();
    }

    private async Task LoadNextItemAsync()
    {
        if (_appState.CurrentSessionId is null)
        {
            await DisplayAlert("Missing session", "There is no active session selected.", "OK");
            await Shell.Current.GoToAsync("//Home");
            return;
        }

        try
        {
            SetLoading(true);
            _currentItem = await _apiService.GetNextItemAsync(_appState.CurrentSessionId.Value);

            if (_currentItem is null)
            {
                await DisplayAlert("Done", "No more suggestions in this session.", "OK");
                await Shell.Current.GoToAsync("//Home");
                return;
            }

            PopulateItem(_currentItem);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void PopulateItem(SessionItemSummary item)
    {
        ItemTitleLabel.Text = item.Title;
        ItemDescriptionLabel.Text = GetText(item.Meta, "description");
        ItemMetaLabel.Text = BuildMetaText(item);
        ItemImage.Source = string.IsNullOrWhiteSpace(item.ImageUrl)
            ? "dotnet_bot.png"
            : ImageSource.FromUri(new Uri(item.ImageUrl));
        SwipeProgressLabel.Text = $"Viewed cards: {_swipeCount}";
    }

    private string BuildMetaText(SessionItemSummary item)
    {
        return item.Category switch
        {
            "Restaurant" => $"{GetText(item.Meta, "cuisine")} - {GetText(item.Meta, "district")} - rating {GetNumber(item.Meta, "rating")}",
            "Recipe" => $"{GetText(item.Meta, "cuisine")} - {GetText(item.Meta, "foodType")} - {GetNumber(item.Meta, "prepTime")} min",
            "BoardGame" => $"{GetText(item.Meta, "gameType")} - {GetNumber(item.Meta, "durationMin")}-{GetNumber(item.Meta, "durationMax")} min - rating {GetNumber(item.Meta, "rating")}",
            _ => $"{JoinArray(item.Meta, "genres")} - {GetNumber(item.Meta, "year")} - rating {GetNumber(item.Meta, "rating")}"
        };
    }

    private async Task SubmitSwipeAsync(bool isYes)
    {
        if (_appState.CurrentSessionId is null || _currentItem is null)
        {
            return;
        }

        try
        {
            SetLoading(true);
            var response = await _apiService.SwipeAsync(_appState.CurrentSessionId.Value, _currentItem.Id, isYes);
            _swipeCount++;

            if (isYes && response.MatchFound)
            {
                var matchedUsers = response.MatchedUsers.Count == 0
                    ? "your group"
                    : string.Join(", ", response.MatchedUsers);

                if (response.FullGroupMatch)
                {
                    _appState.CurrentMatch = _currentItem;
                    _appState.CurrentMatchMessage = $"You all agreed on this choice.";
                    _appState.CurrentMatchedUsers = response.MatchedUsers.ToList();

                    var shouldContinue = await DisplayAlert("Full group match", $"All accepted participants matched on this choice: {matchedUsers}. Do you want to continue swiping for more options?", "Continue", "Stop");
                    if (!shouldContinue)
                    {
                        await Shell.Current.GoToAsync(nameof(MatchPage));
                        return;
                    }
                }
                else
                {
                    await DisplayAlert("Partial match", $"Current match between: {matchedUsers}. The session will continue until everyone agrees or the suggestions finish.", "OK");
                    _appState.CurrentMatchedUsers = response.MatchedUsers.ToList();
                }
            }

            await LoadNextItemAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
    }

    private async void OnRejectClicked(object sender, EventArgs e) => await SubmitSwipeAsync(false);
    private async void OnLikeClicked(object sender, EventArgs e) => await SubmitSwipeAsync(true);
    private async void OnHomeClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//Home");

    private static string GetText(JsonElement meta, string property)
        => meta.ValueKind == JsonValueKind.Object && meta.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? ""
            : "";

    private static string GetNumber(JsonElement meta, string property)
    {
        if (meta.ValueKind != JsonValueKind.Object || !meta.TryGetProperty(property, out var value))
        {
            return "";
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetInt32(out var integer) => integer.ToString(),
            JsonValueKind.Number when value.TryGetDouble(out var floating) => floating.ToString("0.0"),
            _ => ""
        };
    }

    private static string JoinArray(JsonElement meta, string property)
    {
        if (meta.ValueKind != JsonValueKind.Object || !meta.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return "";
        }

        return string.Join(", ", value.EnumerateArray().Select(x => x.GetString()).Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}

