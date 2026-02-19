using System.Text;
using System.Text.Json;

using SwipeMate.Mobile.Services;

namespace SwipeMate.Mobile.Pages;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _auth;
    private readonly ApiClient _api;

    public LoginPage(AuthService auth, ApiClient api)
    {
        InitializeComponent();
        _auth = auth;
        _api = api;
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
            var token = await _auth.LoginAsync(username, password);

            await SecureStorage.SetAsync("jwt", token);
            _api.SetBearer(token);

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

