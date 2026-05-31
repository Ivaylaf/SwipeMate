using System.Net.Mail;
using SwipeMate.Mobile.Services;

namespace SwipeMate.Mobile.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly AuthService _auth;
    private readonly ApiClient _apiClient;
    private bool _isPasswordVisible;

    public RegisterPage(AuthService auth, ApiClient apiClient)
    {
        InitializeComponent();
        _auth = auth;
        _apiClient = apiClient;
        ServerUrlEntry.Text = _apiClient.CurrentBaseUrl;
        UpdatePasswordToggle();
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
            await DisplayAlert("Липсваща информация", "Моля, попълни всички полета.", "OK");
            return;
        }

        if (!IsValidEmail(email))
        {
            await DisplayAlert("Невалиден имейл", "Моля, въведи валиден имейл адрес.", "OK");
            return;
        }

        if (!IsStrongPassword(password))
        {
            await DisplayAlert("Слаба парола", "Паролата трябва да е поне 8 символа и да съдържа малка буква, главна буква, цифра и специален символ.", "OK");
            return;
        }

        try
        {
            _apiClient.UpdateBaseUrl(ServerUrlEntry.Text ?? _apiClient.CurrentBaseUrl);
            await _auth.RegisterAsync(userName, email, password, displayName);
            await DisplayAlert("Успех", "Акаунтът е създаден. Моля, влез в системата.", "OK");
            await Shell.Current.GoToAsync("//Login");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Неуспешна регистрация", ex.Message, "OK");
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

    private static bool IsStrongPassword(string password)
    {
        return password.Length >= 8
            && password.Any(char.IsLower)
            && password.Any(char.IsUpper)
            && password.Any(char.IsDigit)
            && password.Any(ch => !char.IsLetterOrDigit(ch));
    }
}
