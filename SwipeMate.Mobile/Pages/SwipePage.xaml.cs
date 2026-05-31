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
        SwipeTitleLabel.Text = _appState.CurrentCategory ?? "РР·Р±РѕСЂ";
        await LoadNextItemAsync();
    }

    private async Task LoadNextItemAsync()
    {
        if (_appState.CurrentSessionId is null)
        {
            await DisplayAlert("Р›РёРїСЃРІР° СЃРµСЃРёСЏ", "РќСЏРјР° РёР·Р±СЂР°РЅР° Р°РєС‚РёРІРЅР° СЃРµСЃРёСЏ.", "OK");
            await Shell.Current.GoToAsync("//Home");
            return;
        }

        try
        {
            SetLoading(true);
            _currentItem = await _apiService.GetNextItemAsync(_appState.CurrentSessionId.Value);

            if (_currentItem is null)
            {
                await DisplayAlert("Р“РѕС‚РѕРІРѕ", "РќСЏРјР° РїРѕРІРµС‡Рµ РїСЂРµРґР»РѕР¶РµРЅРёСЏ РІ С‚Р°Р·Рё СЃРµСЃРёСЏ. РђРєРѕ РІСЃРёС‡РєРё СѓС‡Р°СЃС‚РЅРёС†Рё СЃР° РїСЂРёРєР»СЋС‡РёР»Рё, СЃРµСЃРёСЏС‚Р° С‰Рµ СЃРµ РїСЂРµРјРµСЃС‚Рё РІ РёСЃС‚РѕСЂРёСЏС‚Р°.", "OK");
                await Shell.Current.GoToAsync("//Home");
                return;
            }

            PopulateItem(_currentItem);
        }
        catch (Exception ex) when (IsInactiveSessionError(ex))
        {
            await DisplayAlert("РЎРµСЃРёСЏС‚Р° РїСЂРёРєР»СЋС‡Рё", "РўР°Р·Рё СЃРµСЃРёСЏ РІРµС‡Рµ РЅРµ Рµ Р°РєС‚РёРІРЅР°. Р©Рµ С‚Рµ РІСЉСЂРЅР° РєСЉРј РЅР°С‡Р°Р»РЅРёСЏ РµРєСЂР°РЅ.", "OK");
            await Shell.Current.GoToAsync("//Home");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Р“СЂРµС€РєР°", ex.Message, "OK");
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
        SwipeProgressLabel.Text = $"РџСЂРµРіР»РµРґР°РЅРё РєР°СЂС‚Рё: {_swipeCount}";
    }

    private string BuildMetaText(SessionItemSummary item)
    {
        return item.Category switch
        {
            "Restaurant" => $"{GetText(item.Meta, "cuisine")} вЂў {GetText(item.Meta, "district")} вЂў РѕС†РµРЅРєР° {GetNumber(item.Meta, "rating")}",
            "Recipe" => $"{GetText(item.Meta, "cuisine")} вЂў {GetText(item.Meta, "foodType")} вЂў {GetNumber(item.Meta, "prepTime")} РјРёРЅ",
            "BoardGame" => $"{GetText(item.Meta, "gameType")} вЂў {GetNumber(item.Meta, "durationMin")}-{GetNumber(item.Meta, "durationMax")} РјРёРЅ вЂў РѕС†РµРЅРєР° {GetNumber(item.Meta, "rating")}",
            _ => $"{JoinArray(item.Meta, "genres")} вЂў {GetNumber(item.Meta, "year")} вЂў РѕС†РµРЅРєР° {GetNumber(item.Meta, "rating")}"
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
                    ? "РІР°С€Р°С‚Р° РіСЂСѓРїР°"
                    : string.Join(", ", response.MatchedUsers);

                if (response.FullGroupMatch)
                {
                    _appState.CurrentMatch = _currentItem;
                    _appState.CurrentMatchMessage = "Р’СЃРёС‡РєРё СЃРµ СЃСЉРіР»Р°СЃРёС…С‚Рµ СЃ С‚РѕР·Рё РёР·Р±РѕСЂ.";
                    _appState.CurrentMatchedUsers = response.MatchedUsers.ToList();

                    var shouldContinue = await DisplayAlert(
                        "РџСЉР»РЅРѕ СЃСЉРІРїР°РґРµРЅРёРµ",
                        $"Р’СЃРёС‡РєРё СѓС‡Р°СЃС‚РЅРёС†Рё СЃСЉРІРїР°РґРЅР°С…Р° РЅР° С‚РѕР·Рё РёР·Р±РѕСЂ: {matchedUsers}. РСЃРєР°С€ Р»Рё РґР° РїСЂРѕРґСЉР»Р¶РёС€ СЃ РѕС‰Рµ РїСЂРµРґР»РѕР¶РµРЅРёСЏ?",
                        "РџСЂРѕРґСЉР»Р¶Рё",
                        "РЎРїСЂРё");

                    if (!shouldContinue)
                    {
                        await Shell.Current.GoToAsync(nameof(MatchPage));
                        return;
                    }
                }
                else
                {
                    await DisplayAlert(
                        "Р§Р°СЃС‚РёС‡РЅРѕ СЃСЉРІРїР°РґРµРЅРёРµ",
                        $"РўРµРєСѓС‰РѕС‚Рѕ СЃСЉРІРїР°РґРµРЅРёРµ Рµ РјРµР¶РґСѓ: {matchedUsers}. РЎРµСЃРёСЏС‚Р° С‰Рµ РїСЂРѕРґСЉР»Р¶Рё, РґРѕРєР°С‚Рѕ РІСЃРёС‡РєРё СЃРµ СЃСЉРіР»Р°СЃСЏС‚ РёР»Рё РїСЂРµРґР»РѕР¶РµРЅРёСЏС‚Р° СЃРІСЉСЂС€Р°С‚.",
                        "OK");
                    _appState.CurrentMatchedUsers = response.MatchedUsers.ToList();
                }
            }

            await LoadNextItemAsync();
        }
        catch (Exception ex) when (IsInactiveSessionError(ex))
        {
            await DisplayAlert("РЎРµСЃРёСЏС‚Р° РїСЂРёРєР»СЋС‡Рё", "РўР°Р·Рё СЃРµСЃРёСЏ РІРµС‡Рµ Рµ РїСЂРёРєР»СЋС‡РёР»Р° РёР»Рё Р·Р°С‚РІРѕСЂРµРЅР°. Р©Рµ С‚Рµ РІСЉСЂРЅР° РєСЉРј РЅР°С‡Р°Р»РЅРёСЏ РµРєСЂР°РЅ.", "OK");
            await Shell.Current.GoToAsync("//Home");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Р“СЂРµС€РєР°", ex.Message, "OK");
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
                "РР·С…РѕРґ РѕС‚ СЃРµСЃРёСЏ",
                "РСЃРєР°С€ Р»Рё РґР° РїСЂРёРєР»СЋС‡РёС€ С‚Р°Р·Рё СЃРµСЃРёСЏ? РђРєРѕ СЏ РїСЂРёРєР»СЋС‡РёС€, С‚СЏ С‰Рµ СЃРµ РїСЂРµРјРµСЃС‚Рё РІ РёСЃС‚РѕСЂРёСЏС‚Р° Рё С‡Р°РєР°С‰РёС‚Рµ РїРѕРєР°РЅРё С‰Рµ Р±СЉРґР°С‚ РѕС‚РјРµРЅРµРЅРё.",
                "РџСЂРёРєР»СЋС‡Рё",
                "РЎР°РјРѕ РёР·Р»РµР·");

            if (closeSession)
            {
                try
                {
                    await _apiService.CloseSessionAsync(sessionId, true);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Р“СЂРµС€РєР°", ex.Message, "OK");
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
               || message.Contains("РЅРµ Рµ Р°РєС‚РёРІ", StringComparison.OrdinalIgnoreCase);
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
