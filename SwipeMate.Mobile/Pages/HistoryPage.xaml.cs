using System.Text;
using System.Text.Json;
using SwipeMate.Mobile.Models;
using SwipeMate.Mobile.Services;

namespace SwipeMate.Mobile.Pages;

public partial class HistoryPage : ContentPage
{
    private readonly SwipeMateApiService _apiService;
    private List<SessionSummary> _sessions = [];

    public HistoryPage(SwipeMateApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            var sessions = await _apiService.GetMySessionsAsync();
            var matches = await _apiService.GetMyMatchesAsync();

            _sessions = sessions
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToList();

            SessionsCollectionView.ItemsSource = _sessions.Select(MapSessionCard).ToList();
            MatchesCollectionView.ItemsSource = matches
                .OrderByDescending(x => x.CreatedAtUtc ?? DateTime.MinValue)
                .Select(MapMatchCard)
                .ToList();
            NoSessionsLabel.IsVisible = _sessions.Count == 0;
            NoMatchesLabel.IsVisible = matches.Count == 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnSessionSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not SessionHistoryCard sessionCard)
        {
            return;
        }

        SessionsCollectionView.SelectedItem = null;

        var session = _sessions.FirstOrDefault(x => x.Id == sessionCard.Id);
        if (session is null)
        {
            return;
        }

        try
        {
            var details = await _apiService.GetSessionDetailsAsync(session.Id);
            var participants = details.Participants.Count == 0
                ? "No participants"
                : string.Join(", ", details.Participants.Select(x => string.IsNullOrWhiteSpace(x.DisplayName) ? x.UserName : $"{x.DisplayName} ({x.UserName})"));

            var message = new StringBuilder()
                .AppendLine($"Status: {details.Status}")
                .AppendLine($"Created: {details.CreatedAtUtc:dd.MM.yyyy HH:mm}")
                .AppendLine($"Participants: {participants}")
                .AppendLine($"Swipes: {details.SwipeCount}")
                .AppendLine($"Matches: {details.MatchCount}")
                .AppendLine($"Pending invitations: {details.PendingInvitationCount}")
                .AppendLine($"Filters: {details.FiltersSummary ?? "No filters saved"}")
                .ToString();

            await DisplayAlert($"{details.Category} session", message, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("..");

    private static SessionHistoryCard MapSessionCard(SessionSummary session)
        => new()
        {
            Id = session.Id,
            Category = GetCategoryTitle(session.Category),
            StatusText = GetStatusTitle(session.Status),
            StatusColor = GetStatusColor(session.Status),
            ParticipantText = $"Participants: {session.ParticipantCount}",
            CreatedText = $"Created: {session.CreatedAtUtc:dd.MM.yyyy HH:mm}"
        };

    private static MatchHistoryCard MapMatchCard(SessionItemSummary item)
    {
        var tags = GetMatchTags(item.Meta, item.Category)
            .Select(x => new TagChip(x))
            .ToList();

        return new MatchHistoryCard
        {
            Title = item.Title,
            ImageSource = string.IsNullOrWhiteSpace(item.ImageUrl) ? "dotnet_bot.png" : item.ImageUrl!,
            DateText = ToRelativeDate(item.CreatedAtUtc),
            RatingText = BuildRatingText(item.Meta, item.Category),
            CategoryBadge = GetCategoryBadge(item.Category),
            CategoryColor = GetCategoryColor(item.Category),
            MatchedUsers = item.MatchedUsers.Select(x => new AvatarChip(x)).ToList(),
            Tags = tags
        };
    }

    private static string BuildRatingText(JsonElement meta, string category)
    {
        var rating = GetNumber(meta, "rating");
        if (string.IsNullOrWhiteSpace(rating))
        {
            return GetCategoryTitle(category);
        }

        return category switch
        {
            "Restaurant" => $"Rating {rating}  •  {GetText(meta, "priceRange")}",
            _ => $"Rating {rating}"
        };
    }

    private static List<string> GetMatchTags(JsonElement meta, string category)
        => category switch
        {
            "Restaurant" => GetStrings(meta, "cuisines").Take(2).ToList(),
            "Recipe" => GetStrings(meta, "ingredients").Take(2).ToList(),
            "BoardGame" => new List<string> { GetText(meta, "gameType") }.Where(x => !string.IsNullOrWhiteSpace(x)).ToList(),
            _ => GetStrings(meta, "genres").Take(2).ToList()
        };

    private static string ToRelativeDate(DateTime? createdAtUtc)
    {
        if (createdAtUtc is null)
        {
            return string.Empty;
        }

        var local = createdAtUtc.Value.ToLocalTime().Date;
        var today = DateTime.Now.Date;
        var days = (today - local).Days;

        if (days <= 0)
        {
            return "Today";
        }

        if (days == 1)
        {
            return "Yesterday";
        }

        if (days < 7)
        {
            return $"{days} days ago";
        }

        return createdAtUtc.Value.ToLocalTime().ToString("dd.MM.yyyy");
    }

    private static string GetCategoryTitle(string category)
        => category switch
        {
            "Movie" => "Movies & TV",
            "Restaurant" => "Restaurants",
            "Recipe" => "Recipes",
            "BoardGame" => "Board Games",
            _ => category
        };

    private static string GetStatusTitle(string status)
        => status switch
        {
            "Active" => "Active",
            "Pending" => "Pending invitations",
            "Finished" => "Finished",
            "Partial" => "Partially accepted",
            "Closed" => "Closed",
            "Declined" => "Declined",
            _ => status
        };

    private static string GetStatusColor(string status)
        => status switch
        {
            "Active" => "#C026D3",
            "Pending" => "#D97706",
            "Finished" => "#059669",
            "Partial" => "#7C3AED",
            "Closed" => "#6B7280",
            "Declined" => "#DC2626",
            _ => "#6B7280"
        };

    private static string GetCategoryBadge(string category)
        => category switch
        {
            "Movie" => "TV",
            "Restaurant" => "RS",
            "Recipe" => "RC",
            "BoardGame" => "BG",
            _ => "SM"
        };

    private static string GetCategoryColor(string category)
        => category switch
        {
            "Movie" => "#0EA5E9",
            "Restaurant" => "#F97316",
            "Recipe" => "#22C55E",
            "BoardGame" => "#A855F7",
            _ => "#C026D3"
        };

    private static string GetText(JsonElement meta, string property)
        => meta.ValueKind == JsonValueKind.Object && meta.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    private static string GetNumber(JsonElement meta, string property)
    {
        if (meta.ValueKind != JsonValueKind.Object || !meta.TryGetProperty(property, out var value))
        {
            return string.Empty;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetInt32(out var integer) => integer.ToString(),
            JsonValueKind.Number when value.TryGetDouble(out var floating) => floating.ToString("0.0"),
            _ => string.Empty
        };
    }

    private static IEnumerable<string> GetStrings(JsonElement meta, string property)
    {
        if (meta.ValueKind != JsonValueKind.Object || !meta.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return value.EnumerateArray().Select(x => x.GetString() ?? string.Empty).Where(x => !string.IsNullOrWhiteSpace(x));
    }

    private sealed class SessionHistoryCard
    {
        public Guid Id { get; init; }
        public string Category { get; init; } = string.Empty;
        public string StatusText { get; init; } = string.Empty;
        public string StatusColor { get; init; } = "#6B7280";
        public string ParticipantText { get; init; } = string.Empty;
        public string CreatedText { get; init; } = string.Empty;
    }

    private sealed class MatchHistoryCard
    {
        public string Title { get; init; } = string.Empty;
        public string ImageSource { get; init; } = "dotnet_bot.png";
        public string DateText { get; init; } = string.Empty;
        public string RatingText { get; init; } = string.Empty;
        public string CategoryBadge { get; init; } = "SM";
        public string CategoryColor { get; init; } = "#C026D3";
        public bool HasMatchedUsers => MatchedUsers.Count > 0;
        public List<AvatarChip> MatchedUsers { get; init; } = [];
        public List<TagChip> Tags { get; init; } = [];
    }

    private sealed class AvatarChip(string name)
    {
        public string Name { get; } = name;
        public string Initials => string.Concat(Name.Split([' ', '_', '-'], StringSplitOptions.RemoveEmptyEntries).Select(x => x[0]).Take(2)).ToUpperInvariant();
    }

    private sealed class TagChip(string text)
    {
        public string Text { get; } = text;
    }
}


