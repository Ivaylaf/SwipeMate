using SwipeMate.Mobile.Models;
using SwipeMate.Mobile.Services;

namespace SwipeMate.Mobile.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly AppState _appState;
    private readonly SwipeMateApiService _apiService;

    public ProfilePage(AppState appState, SwipeMateApiService apiService)
    {
        InitializeComponent();
        _appState = appState;
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        UserNameLabel.Text = _appState.User?.UserName ?? "Потребител";
        AdminRoleBadge.IsVisible = _appState.User?.IsAdmin == true;
        EmailLabel.Text = _appState.User?.Email ?? "Няма наличен имейл";

        var initials = (_appState.User?.UserName ?? "SM").Trim();
        AvatarLabel.Text = initials.Length >= 2
            ? initials[..2].ToUpperInvariant()
            : initials.ToUpperInvariant();

        try
        {
            var profile = await _apiService.GetProfileAsync();
            ApplyProfile(profile);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("Неоторизиран", StringComparison.OrdinalIgnoreCase))
            {
                MatchesCountLabel.Text = "0";
                SessionsCountLabel.Text = "0";
                RatingsCountLabel.Text = "0";
                MatchProgressLabel.Text = "0/25 съвпадения";
                SessionProgressLabel.Text = "0/50 сесии";
                RatingProgressLabel.Text = "0/100 действия";
                return;
            }

            await DisplayAlert("Грешка", ex.Message, "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("..");

    private async void OnSaveProfileClicked(object sender, EventArgs e)
    {
        try
        {
            var profileImageUrl = NormalizeProfileImageUrl(ProfileImageUrlEntry.Text);
            if (!string.IsNullOrWhiteSpace(ProfileImageUrlEntry.Text) && profileImageUrl is null)
            {
                ShowProfileImageStatus("Моля, въведи валиден http/https линк към изображение.", true);
                return;
            }

            var profile = await _apiService.UpdateProfileAsync(
                DisplayNameEntry.Text ?? "",
                BioEditor.Text ?? "",
                profileImageUrl ?? "");

            ApplyProfile(profile);
            _appState.UpdateProfile(profile.DisplayName, profile.Email);
            await DisplayAlert("Запазено", "Профилът беше обновен успешно.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Профилът не можа да бъде запазен", ex.Message, "OK");
        }
    }

    private void OnProfileImageUrlChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            ApplyProfileImage(null);
            ProfileImageStatusLabel.IsVisible = false;
        }
    }

    private void OnPreviewImageClicked(object sender, EventArgs e)
    {
        var profileImageUrl = NormalizeProfileImageUrl(ProfileImageUrlEntry.Text);
        if (profileImageUrl is null)
        {
            ApplyProfileImage(null);
            ShowProfileImageStatus("Постави валиден http/https линк към изображение.", true);
            return;
        }

        ApplyProfileImage(profileImageUrl);
        ShowProfileImageStatus("Прегледът е зареден. Натисни „Запази профила“, за да остане снимката.", false);
    }

    private void OnClearProfileImageClicked(object sender, EventArgs e)
    {
        ProfileImageUrlEntry.Text = "";
        ApplyProfileImage(null);
        ShowProfileImageStatus("Снимката е премахната. Натисни „Запази профила“, за да запазиш промяната.", false);
    }

    private static string? NormalizeProfileImageUrl(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            return null;
        }

        return uri.Scheme is "http" or "https" ? trimmed : null;
    }

    private void ApplyProfileImage(string? imageUrl)
    {
        var normalizedUrl = NormalizeProfileImageUrl(imageUrl);
        if (normalizedUrl is null)
        {
            ProfileImage.Source = null;
            ProfileImage.IsVisible = false;
            AvatarLabel.IsVisible = true;
            return;
        }

        ProfileImage.Source = ImageSource.FromUri(new Uri(normalizedUrl));
        ProfileImage.IsVisible = true;
        AvatarLabel.IsVisible = false;
    }

    private void ShowProfileImageStatus(string message, bool isError)
    {
        ProfileImageStatusLabel.Text = message;
        ProfileImageStatusLabel.TextColor = Color.FromArgb(isError ? "#DC2626" : "#6B7280");
        ProfileImageStatusLabel.IsVisible = true;
    }
    private void ApplyProfile(ProfileSummary profile)
    {
        UserNameLabel.Text = profile.UserName;
        EmailLabel.Text = profile.Email ?? "Няма наличен имейл";
        BioPreviewLabel.Text = string.IsNullOrWhiteSpace(profile.Bio)
            ? "Обича филми и храна! Винаги търси нови преживявания."
            : profile.Bio;
        DisplayNameEntry.Text = profile.DisplayName;
        BioEditor.Text = profile.Bio;
        ProfileImageUrlEntry.Text = profile.ProfileImageUrl;
        ApplyProfileImage(profile.ProfileImageUrl);
        ProfileImageStatusLabel.IsVisible = false;

        MatchesCountLabel.Text = profile.MatchesCount.ToString();
        SessionsCountLabel.Text = profile.SessionsCount.ToString();
        RatingsCountLabel.Text = profile.RatingsCount.ToString();

        MatchProgress.Progress = Math.Min(profile.MatchesCount / 25.0, 1.0);
        SessionProgress.Progress = Math.Min(profile.SessionsCount / 50.0, 1.0);
        RatingProgress.Progress = Math.Min(profile.RatingsCount / 100.0, 1.0);

        MatchProgressLabel.Text = $"{profile.MatchesCount}/25 съвпадения";
        SessionProgressLabel.Text = $"{profile.SessionsCount}/50 сесии";
        RatingProgressLabel.Text = $"{profile.RatingsCount}/100 действия";
    }
}
