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

        UserNameLabel.Text = _appState.User?.UserName ?? "User";
        AdminRoleBadge.IsVisible = _appState.User?.IsAdmin == true;
        EmailLabel.Text = _appState.User?.Email ?? "No email available";

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
            if (ex.Message.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase))
            {
                MatchesCountLabel.Text = "0";
                SessionsCountLabel.Text = "0";
                RatingsCountLabel.Text = "0";
                MatchProgressLabel.Text = "0/25 matches";
                SessionProgressLabel.Text = "0/50 sessions";
                RatingProgressLabel.Text = "0/100 actions";
                return;
            }

            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("..");

    private async void OnSaveProfileClicked(object sender, EventArgs e)
    {
        try
        {
            var profile = await _apiService.UpdateProfileAsync(
                DisplayNameEntry.Text ?? "",
                BioEditor.Text ?? "",
                ProfileImageUrlEntry.Text ?? "");

            ApplyProfile(profile);
            _appState.UpdateProfile(profile.DisplayName, profile.Email);
            await DisplayAlert("Saved", "Profile updated successfully.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Could not save profile", ex.Message, "OK");
        }
    }

    private void ApplyProfile(ProfileSummary profile)
    {
        UserNameLabel.Text = profile.UserName;
        EmailLabel.Text = profile.Email ?? "No email available";
        BioPreviewLabel.Text = string.IsNullOrWhiteSpace(profile.Bio)
            ? "Love movies and food! Always looking for new experiences."
            : profile.Bio;
        DisplayNameEntry.Text = profile.DisplayName;
        BioEditor.Text = profile.Bio;
        ProfileImageUrlEntry.Text = profile.ProfileImageUrl;
        if (Uri.TryCreate(profile.ProfileImageUrl, UriKind.Absolute, out var imageUri))
        {
            ProfileImage.Source = ImageSource.FromUri(imageUri);
            ProfileImage.IsVisible = true;
            AvatarLabel.IsVisible = false;
        }
        else
        {
            ProfileImage.Source = null;
            ProfileImage.IsVisible = false;
            AvatarLabel.IsVisible = true;
        }

        MatchesCountLabel.Text = profile.MatchesCount.ToString();
        SessionsCountLabel.Text = profile.SessionsCount.ToString();
        RatingsCountLabel.Text = profile.RatingsCount.ToString();

        MatchProgress.Progress = Math.Min(profile.MatchesCount / 25.0, 1.0);
        SessionProgress.Progress = Math.Min(profile.SessionsCount / 50.0, 1.0);
        RatingProgress.Progress = Math.Min(profile.RatingsCount / 100.0, 1.0);

        MatchProgressLabel.Text = $"{profile.MatchesCount}/25 matches";
        SessionProgressLabel.Text = $"{profile.SessionsCount}/50 sessions";
        RatingProgressLabel.Text = $"{profile.RatingsCount}/100 actions";
    }
}

