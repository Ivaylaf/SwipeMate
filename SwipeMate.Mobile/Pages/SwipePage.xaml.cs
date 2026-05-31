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
        SwipeTitleLabel.Text = _appState.CurrentCategory ?? "Избор";
        await LoadNextItemAsync();
    }

    private async Task LoadNextItemAsync()
    {
        if (_appState.CurrentSessionId is null)
        {
            await DisplayAlert("Липсва сесия", "Няма избрана активна сесия.", "OK");
            await Shell.Current.GoToAsync("//Home");
            return;
        }

        try
        {
            SetLoading(true);
            _currentItem = await _apiService.GetNextItemAsync(_appState.CurrentSessionId.Value);

            if (_currentItem is null)
            {
                await DisplayAlert("Готово", "Няма повече предложения в тази сесия. Ако всички участници са приключили, сесията ще се премести в историята.", "OK");
                await Shell.Current.GoToAsync("//Home");
                return;
            }

            PopulateItem(_currentItem);
        }
        catch (Exception ex) when (IsInactiveSessionError(ex))
        {
            await DisplayAlert("Сесията приключи", "Тази сесия вече не е активна. Ще те върна към началния екран.", "OK");
            await Shell.Current.GoToAsync("//Home");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Грешка", ex.Message, "OK");
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
        SwipeProgressLabel.Text = $"Прегледани карти: {_swipeCount}";
    }

    private string BuildMetaText(SessionItemSummary item)
    {
        return item.Category switch
        {
            "Restaurant" => $"{GetText(item.Meta, "cuisine")} • {GetText(item.Meta, "district")} • оценка {GetNumber(item.Meta, "rating")}",
            "Recipe" => $"{GetText(item.Meta, "cuisine")} • {GetText(item.Meta, "foodType")} • {GetNumber(item.Meta, "prepTime")} мин",
            "BoardGame" => $"{GetText(item.Meta, "gameType")} • {GetNumber(item.Meta, "durationMin")}-{GetNumber(item.Meta, "durationMax")} мин • оценка {GetNumber(item.Meta, "rating")}",
            _ => $"{JoinArray(item.Meta, "genres")} • {GetNumber(item.Meta, "year")} • оценка {GetNumber(item.Meta, "rating")}"
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
                    ? "вашата група"
                    : string.Join(", ", response.MatchedUsers);

                if (response.FullGroupMatch)
                {
                    _appState.CurrentMatch = _currentItem;
                    _appState.CurrentMatchMessage = "Всички се съгласихте с този избор.";
                    _appState.CurrentMatchedUsers = response.MatchedUsers.ToList();

                    var shouldContinue = await DisplayAlert(
                        "Пълно съвпадение",
                        $"Всички участници съвпаднаха на този избор: {matchedUsers}. Искаш ли да продължиш с още предложения?",
                        "Продължи",
                        "Спри");

                    if (!shouldContinue)
                    {
                        await Shell.Current.GoToAsync(nameof(MatchPage));
                        return;
                    }
                }
                else
                {
                    await DisplayAlert(
                        "Частично съвпадение",
                        $"Текущото съвпадение е между: {matchedUsers}. Сесията ще продължи, докато всички се съгласят или предложенията свършат.",
                        "OK");
                    _appState.CurrentMatchedUsers = response.MatchedUsers.ToList();
                }
            }

            await LoadNextItemAsync();
        }
        catch (Exception ex) when (IsInactiveSessionError(ex))
        {
            await DisplayAlert("Сесията приключи", "Тази сесия вече е приключила или затворена. Ще те върна към началния екран.", "OK");
            await Shell.Current.GoToAsync("//Home");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Грешка", ex.Message, "OK");
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

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        if (_appState.CurrentSessionId is Guid sessionId && _appState.CurrentSessionIsOwner)
        {
            var closeSession = await DisplayAlert(
                "Изход от сесия",
                "Искаш ли да приключиш тази сесия? Ако я приключиш, тя ще се премести в историята и чакащите покани ще бъдат отменени.",
                "Приключи",
                "Само излез");

            if (closeSession)
            {
                try
                {
                    await _apiService.CloseSessionAsync(sessionId, true);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Грешка", ex.Message, "OK");
                    return;
                }
            }
        }

        await Shell.Current.GoToAsync("//Home");
    }

    private static bool IsInactiveSessionError(Exception ex)
    {
        var message = ex.Message ?? string.Empty;
        return message.Contains("not active", StringComparison.OrdinalIgnoreCase)
               || message.Contains("not active yet", StringComparison.OrdinalIgnoreCase)
               || message.Contains("не е актив", StringComparison.OrdinalIgnoreCase);
    }
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