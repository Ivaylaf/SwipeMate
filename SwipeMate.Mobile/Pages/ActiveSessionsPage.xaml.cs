using SwipeMate.Mobile.Models;
using SwipeMate.Mobile.Services;

namespace SwipeMate.Mobile.Pages;

public partial class ActiveSessionsPage : ContentPage
{
    private readonly SwipeMateApiService _apiService;
    private readonly AppState _appState;
    private List<SessionSummary> _currentSessions = [];
    private List<SessionInvitationSummary> _pendingInvitations = [];

    public ActiveSessionsPage(SwipeMateApiService apiService, AppState appState)
    {
        InitializeComponent();
        _apiService = apiService;
        _appState = appState;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSessionsAsync();
    }

    private async Task LoadSessionsAsync()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var invitations = await _apiService.GetSessionInvitationsAsync();
            _pendingInvitations = invitations
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToList();

            var sessions = await _apiService.GetMySessionsAsync();
            _currentSessions = sessions
                .Where(x => SwipeMateDisplayText.IsCurrentSessionStatus(x.Status))
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToList();

            InvitationsCollectionView.ItemsSource = _pendingInvitations;
            NoInvitationsLabel.IsVisible = _pendingInvitations.Count == 0;

            SessionsCollectionView.ItemsSource = _currentSessions;
            NoSessionsLabel.IsVisible = _currentSessions.Count == 0;
            LastRefreshLabel.Text = $"РџРѕСЃР»РµРґРЅРѕ РѕР±РЅРѕРІСЏРІР°РЅРµ: {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Р“СЂРµС€РєР°", ex.Message, "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async void OnAcceptInvitationClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not SessionInvitationSummary invitation)
        {
            return;
        }

        try
        {
            await _apiService.RespondToSessionInvitationAsync(invitation.Id, true);
            await LoadSessionsAsync();
            await DisplayAlert("РџСЂРёРµС‚Р° РїРѕРєР°РЅР°", "РџРѕРєР°РЅР°С‚Р° Р±РµС€Рµ РїСЂРёРµС‚Р°. РћС‚РІРѕСЂРё вЂћРњРѕРёС‚Рµ С„РёР»С‚СЂРёвЂњ, Р·Р° РґР° РґРѕР±Р°РІРёС€ РїСЂРµРґРїРѕС‡РёС‚Р°РЅРёСЏС‚Р° СЃРё РїСЂРµРґРё РіР»Р°СЃСѓРІР°РЅРµС‚Рѕ.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Р“СЂРµС€РєР°", ex.Message, "OK");
        }
    }

    private async void OnDeclineInvitationClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not SessionInvitationSummary invitation)
        {
            return;
        }

        try
        {
            await _apiService.RespondToSessionInvitationAsync(invitation.Id, false);
            await LoadSessionsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Р“СЂРµС€РєР°", ex.Message, "OK");
        }
    }

    private async void OnOpenSessionClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not SessionSummary session)
        {
            return;
        }

        if (!string.Equals(session.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            await DisplayAlert("РЎРµСЃРёСЏС‚Р° РѕС‰Рµ РЅРµ Рµ РіРѕС‚РѕРІР°", "РўР°Р·Рё СЃРµСЃРёСЏ РѕС‰Рµ РЅРµ Рµ Р°РєС‚РёРІРЅР°. РР·С‡Р°РєР°Р№ РїРѕРєР°РЅРёС‚Рµ РґР° Р±СЉРґР°С‚ РїСЂРёРµС‚Рё РёР»Рё РїСЉСЂРІРѕ Р·Р°РїР°Р·Рё РїСЂРµРґРїРѕС‡РёС‚Р°РЅРёСЏС‚Р° СЃРё С‡СЂРµР· вЂћРњРѕРёС‚Рµ С„РёР»С‚СЂРёвЂњ.", "OK");
            return;
        }

        _appState.CurrentSessionId = session.Id;
        _appState.CurrentCategory = session.Category;
        _appState.CurrentSessionIsOwner = session.IsOwner;
        _appState.CurrentMatch = null;

        await Shell.Current.GoToAsync(nameof(SwipePage));
    }

    private async void OnSessionFiltersClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not SessionSummary session)
        {
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(CreateSessionPage)}?category={Uri.EscapeDataString(session.Category)}&sessionId={session.Id}");
    }

    private async void OnCloseSessionClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not SessionSummary session)
        {
            return;
        }

        var confirm = await DisplayAlert(
            "РџСЂРёРєР»СЋС‡РІР°РЅРµ РЅР° СЃРµСЃРёСЏ",
            "РСЃРєР°С€ Р»Рё РґР° РїСЂРёРєР»СЋС‡РёС€ С‚Р°Р·Рё СЃРµСЃРёСЏ? РўСЏ С‰Рµ СЃРµ РїСЂРµРјРµСЃС‚Рё РІ РёСЃС‚РѕСЂРёСЏС‚Р° Рё С‡Р°РєР°С‰РёС‚Рµ РїРѕРєР°РЅРё С‰Рµ Р±СЉРґР°С‚ РѕС‚РјРµРЅРµРЅРё.",
            "РџСЂРёРєР»СЋС‡Рё",
            "РћС‚РєР°Р·");

        if (!confirm)
        {
            return;
        }

        try
        {
            await _apiService.CloseSessionAsync(session.Id, true);
            await LoadSessionsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Р“СЂРµС€РєР°", ex.Message, "OK");
        }
    }

    private async void OnRefreshClicked(object sender, EventArgs e) => await LoadSessionsAsync();
    private async void OnBackClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("..");
}

