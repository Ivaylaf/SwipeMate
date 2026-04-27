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
                .Where(x => !string.Equals(x.Status, "Finished", StringComparison.OrdinalIgnoreCase)
                         && !string.Equals(x.Status, "Closed", StringComparison.OrdinalIgnoreCase)
                         && !string.Equals(x.Status, "Declined", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToList();

            InvitationsCollectionView.ItemsSource = _pendingInvitations;
            NoInvitationsLabel.IsVisible = _pendingInvitations.Count == 0;

            SessionsCollectionView.ItemsSource = _currentSessions;
            NoSessionsLabel.IsVisible = _currentSessions.Count == 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
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
            await DisplayAlert("Joined", "You accepted the invitation. Open 'My Filters' to add your own filters before swiping.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
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
            await DisplayAlert("Error", ex.Message, "OK");
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
            await DisplayAlert("Session not ready", "This session is not active yet. Wait for invitations to be accepted, or use 'My Filters' to save your preferences first.", "OK");
            return;
        }

        _appState.CurrentSessionId = session.Id;
        _appState.CurrentCategory = session.Category;
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

    private async void OnRefreshClicked(object sender, EventArgs e) => await LoadSessionsAsync();
    private async void OnBackClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("..");
}
