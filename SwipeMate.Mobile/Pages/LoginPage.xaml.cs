using SwipeMate.Mobile.Services;

namespace SwipeMate.Mobile.Pages;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _auth;
    private readonly AppState _appState;
    private readonly ApiClient _apiClient;

    public LoginPage(AuthService auth, AppState appState, ApiClient apiClient)
    {
        InitializeComponent();
        _auth = auth;
        _appState = appState;
        _apiClient = apiClient;
        ServerUrlEntry.Text = _apiClient.CurrentBaseUrl;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text?.Trim() ?? "";
        var password = PasswordEntry.Text ?? "";

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Missing info", "Please enter username and password.", "OK");
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
            await DisplayAlert("Login failed", ex.Message, "OK");
        }
    }

    private async void OnGoRegisterClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//Register");
}
