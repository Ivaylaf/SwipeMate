using System.Net.Mail;
using SwipeMate.Mobile.Services;

namespace SwipeMate.Mobile.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly AuthService _auth;
    private readonly ApiClient _apiClient;

    public RegisterPage(AuthService auth, ApiClient apiClient)
    {
        InitializeComponent();
        _auth = auth;
        _apiClient = apiClient;
        ServerUrlEntry.Text = _apiClient.CurrentBaseUrl;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var userName = UserNameEntry.Text?.Trim() ?? "";
        var email = EmailEntry.Text?.Trim() ?? "";
        var displayName = DisplayNameEntry.Text?.Trim() ?? "";
        var password = PasswordEntry.Text ?? "";

        if (string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(displayName) ||
            string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Missing info", "Please fill all fields.", "OK");
            return;
        }

        if (!IsValidEmail(email))
        {
            await DisplayAlert("Invalid email", "Please enter a valid email address.", "OK");
            return;
        }

        try
        {
            _apiClient.UpdateBaseUrl(ServerUrlEntry.Text ?? _apiClient.CurrentBaseUrl);
            await _auth.RegisterAsync(userName, email, password, displayName);
            await DisplayAlert("Success", "Account created. Please log in.", "OK");
            await Shell.Current.GoToAsync("//Login");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Register failed", ex.Message, "OK");
        }
    }

    private async void OnGoLoginClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//Login");

    private static bool IsValidEmail(string email)
    {
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
