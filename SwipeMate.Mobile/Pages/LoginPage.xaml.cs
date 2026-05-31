using SwipeMate.Mobile.Services;

namespace SwipeMate.Mobile.Pages;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _auth;
    private readonly AppState _appState;
    private readonly ApiClient _apiClient;
    private bool _isPasswordVisible;

    public LoginPage(AuthService auth, AppState appState, ApiClient apiClient)
    {
        InitializeComponent();
        _auth = auth;
        _appState = appState;
        _apiClient = apiClient;
        ServerUrlEntry.Text = _apiClient.CurrentBaseUrl;
        UpdatePasswordToggle();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (!_appState.IsAuthenticated)
        {
            UsernameEntry.Text = string.Empty;
            PasswordEntry.Text = string.Empty;
            _isPasswordVisible = false;
            PasswordEntry.IsPassword = true;
            UpdatePasswordToggle();
        }
    }

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordEntry.IsPassword = !_isPasswordVisible;
        UpdatePasswordToggle();
    }

    private void UpdatePasswordToggle()
    {
        TogglePasswordButton.Text = _isPasswordVisible ? "Скрий" : "Покажи";
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text?.Trim() ?? "";
        var password = PasswordEntry.Text ?? "";

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Липсваща информация", "Моля, въведи потребителско име и парола.", "OK");
            return;
        }

        try
        {
            _apiClient.UpdateBaseUrl(ServerUrlEntry.Text ?? _apiClient.CurrentBaseUrl);
            var token = await _auth.LoginAsync(username, password);
            await _appState.SetAuthenticatedAsync(token);
            await Shell.Current.GoToAsync("//Home");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Неуспешен вход", ex.Message, "OK");
        }
    }

    private async void OnGoRegisterClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//Register");
}

