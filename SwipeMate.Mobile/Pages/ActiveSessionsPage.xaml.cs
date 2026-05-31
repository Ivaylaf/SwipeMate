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
            LastRefreshLabel.Text = $"Последно обновяване: {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Грешка", ex.Message, "OK");
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
            await DisplayAlert("Приета покана", "Поканата беше приета. Отвори „Моите филтри“, за да добавиш предпочитанията си преди гласуването.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Грешка", ex.Message, "OK");
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
            await DisplayAlert("Грешка", ex.Message, "OK");
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
            await DisplayAlert("Сесията още не е готова", "Тази сесия още не е активна. Изчакай поканите да бъдат приети или първо запази предпочитанията си чрез „Моите филтри“.", "OK");
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
            "Приключване на сесия",
            "Искаш ли да приключиш тази сесия? Тя ще се премести в историята и чакащите покани ще бъдат отменени.",
            "Приключи",
            "Отказ");

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
            await DisplayAlert("Грешка", ex.Message, "OK");
        }
    }

    private async void OnRefreshClicked(object sender, EventArgs e) => await LoadSessionsAsync();
    private async void OnBackClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("..");
}
