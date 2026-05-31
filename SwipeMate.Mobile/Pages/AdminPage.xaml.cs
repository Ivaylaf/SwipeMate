using System.Text;
using SwipeMate.Mobile.Models;
using SwipeMate.Mobile.Services;

namespace SwipeMate.Mobile.Pages;

public partial class AdminPage : ContentPage
{
    private readonly AppState _appState;
    private readonly SwipeMateApiService _apiService;

    public AdminPage(AppState appState, SwipeMateApiService apiService)
    {
        InitializeComponent();
        _appState = appState;
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var isAdmin = _appState.User?.IsAdmin == true;
        AccessDeniedLabel.IsVisible = !isAdmin;
        AdminContent.IsVisible = isAdmin;

        if (!isAdmin)
        {
            return;
        }

        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var users = await _apiService.GetAdminUsersAsync();
            var catalog = await _apiService.GetAdminCatalogAsync();

            UsersCollectionView.ItemsSource = users;
            CatalogCollectionView.ItemsSource = catalog;
            UsersCountLabel.Text = $"{users.Count} общо";
            CatalogCountLabel.Text = $"{catalog.Count(x => x.IsActive)} активни / {catalog.Count} общо";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Админ данните не могат да се заредят", ex.Message, "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async void OnUserDetailsClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not AdminUserSummary user)
        {
            return;
        }

        try
        {
            var details = await _apiService.GetAdminUserDetailsAsync(user.Id);
            var message = new StringBuilder()
                .AppendLine($"Потребител: {details.UserName}")
                .AppendLine($"Показвано име: {details.DisplayName ?? "-"}")
                .AppendLine($"Имейл: {details.Email ?? "-"}")
                .AppendLine($"Роли: {(details.Roles.Count == 0 ? "User" : string.Join(", ", details.Roles))}")
                .AppendLine($"Статус: {(details.IsBlocked ? "Блокиран" : "Активен")}")
                .AppendLine($"Причина: {details.BlockedReason ?? "-"}")
                .AppendLine($"Приятели: {details.FriendsCount}")
                .AppendLine($"Сесии: {details.SessionsCount}")
                .AppendLine($"Съвпадения: {details.MatchesCount}")
                .AppendLine($"Снимка: {details.ProfileImageUrl ?? "няма зададен линк"}")
                .AppendLine()
                .AppendLine($"Описание: {details.Bio ?? "няма описание"}")
                .AppendLine()
                .AppendLine("Последни сесии:");

            if (details.RecentSessions.Count == 0)
            {
                message.AppendLine("- няма записи");
            }
            else
            {
                foreach (var session in details.RecentSessions)
                {
                    message.AppendLine($"- {session.Display}");
                }
            }

            message.AppendLine().AppendLine("Последни съвпадения:");
            if (details.RecentMatches.Count == 0)
            {
                message.AppendLine("- няма записи");
            }
            else
            {
                foreach (var match in details.RecentMatches)
                {
                    message.AppendLine($"- {match.Display}");
                }
            }

            await DisplayAlert("Детайли за потребител", message.ToString(), "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Детайлите не могат да се заредят", ex.Message, "OK");
        }
    }

    private async void OnBlockToggleClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not AdminUserSummary user)
        {
            return;
        }

        try
        {
            if (user.IsBlocked)
            {
                await _apiService.UnblockAdminUserAsync(user.Id);
            }
            else
            {
                var reason = await DisplayPromptAsync("Блокиране на потребител", $"Причина за блокиране на {user.UserName}:", "Блокирай", "Отказ", "Например: нарушение на правилата");
                if (reason is null)
                {
                    return;
                }

                await _apiService.BlockAdminUserAsync(user.Id, reason);
            }

            await LoadAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Действието не беше изпълнено", ex.Message, "OK");
        }
    }

    private async void OnCatalogDetailsClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not AdminCatalogItemSummary item)
        {
            return;
        }

        try
        {
            var details = await _apiService.GetAdminCatalogDetailsAsync(item.Id);
            var message = new StringBuilder()
                .AppendLine($"Категория: {details.Category}")
                .AppendLine($"Външен идентификатор: {details.ExternalId}")
                .AppendLine($"Статус: {(details.IsActive ? "Активно" : "Скрито")}")
                .AppendLine($"Добавено: {details.CreatedAtUtc:dd.MM.yyyy HH:mm}")
                .AppendLine($"Обобщение: {details.Summary ?? "-"}")
                .AppendLine($"Описание: {details.Description ?? "-"}")
                .AppendLine($"Източник: {details.SourceName ?? "-"}")
                .AppendLine($"Линк: {details.SourceUrl ?? "-"}")
                .AppendLine($"Снимка: {details.ImageUrl ?? "-"}");

            await DisplayAlert("Детайли за елемент", message.ToString(), "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Детайлите не могат да се заредят", ex.Message, "OK");
        }
    }

    private async void OnCatalogToggleClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not AdminCatalogItemSummary item)
        {
            return;
        }

        try
        {
            await _apiService.SetAdminCatalogStatusAsync(item.Id, !item.IsActive);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Каталогът не беше обновен", ex.Message, "OK");
        }
    }

    private async void OnBackupClicked(object sender, EventArgs e)
    {
        try
        {
            var json = await _apiService.GetAdminBackupJsonAsync();
            var path = Path.Combine(FileSystem.CacheDirectory, $"swipemate-backup-{DateTime.Now:yyyyMMdd-HHmmss}.json");
            await File.WriteAllTextAsync(path, json);
            await DisplayAlert("Backup готов", $"Файлът е записан тук:\n{path}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Backup не беше създаден", ex.Message, "OK");
        }
    }

    private async void OnRefreshClicked(object sender, EventArgs e) => await LoadAsync();
    private async void OnBackClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("..");
}
