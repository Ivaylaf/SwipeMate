using System.Text.Json;

namespace SwipeMate.Mobile.Models;

public sealed class CurrentUser
{
    public string UserName { get; set; } = "";
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = [];
    public bool IsAdmin => Roles.Any(role => string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase));
}

public sealed class FriendSummary
{
    public string Id { get; set; } = "";
    public string UserName { get; set; } = "";
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string AvatarInitials =>
        string.IsNullOrWhiteSpace(UserName)
            ? "SM"
            : string.Concat(UserName.Trim().Split(['_', '-', ' '], StringSplitOptions.RemoveEmptyEntries).Select(x => x[0]).Take(2)).ToUpperInvariant();
}

public sealed class FriendRequestSummary
{
    public Guid Id { get; set; }
    public string FromUserId { get; set; } = "";
    public string? UserName { get; set; }
    public string? DisplayName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string AvatarInitials =>
        string.IsNullOrWhiteSpace(UserName)
            ? "SM"
            : string.Concat(UserName.Trim().Split(['_', '-', ' '], StringSplitOptions.RemoveEmptyEntries).Select(x => x[0]).Take(2)).ToUpperInvariant();
}

public sealed class SessionSummary
{
    public Guid Id { get; set; }
    public string Category { get; set; } = "";
    public string Status { get; set; } = "";
    public string CreatedByUserId { get; set; } = "";
    public bool IsOwner { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int ParticipantCount { get; set; }
    public string CategoryDisplayName => SwipeMateDisplayText.Category(Category);
    public string StatusDisplayName => SwipeMateDisplayText.Status(Status);
    public string StatusColor => SwipeMateDisplayText.StatusColor(Status);
    public bool CanClose => IsOwner && SwipeMateDisplayText.IsCurrentSessionStatus(Status);
}

public sealed class SessionItemSummary
{
    public Guid? SessionId { get; set; }
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string? ImageUrl { get; set; }
    public JsonElement Meta { get; set; }
    public DateTime? CreatedAtUtc { get; set; }
    public List<string> MatchedUsers { get; set; } = [];
}

public sealed class CreateSessionResponse
{
    public Guid SessionId { get; set; }
    public string Category { get; set; } = "";
}

public sealed class SwipeResponse
{
    public bool Ok { get; set; }
    public bool MatchFound { get; set; }
    public bool FullGroupMatch { get; set; }
    public string? SessionStatus { get; set; }
    public List<string> MatchedUsers { get; set; } = [];
}

public sealed class ProfileSummary
{
    public string Id { get; set; } = "";
    public string UserName { get; set; } = "";
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }
    public int MatchesCount { get; set; }
    public int SessionsCount { get; set; }
    public int RatingsCount { get; set; }
}

public sealed class CatalogOptionsSummary
{
    public MovieCatalogOptions Movies { get; set; } = new();
    public RestaurantCatalogOptions Restaurants { get; set; } = new();
    public RecipeCatalogOptions Recipes { get; set; } = new();
    public BoardGameCatalogOptions BoardGames { get; set; } = new();
}

public sealed class MovieCatalogOptions
{
    public List<string> Genres { get; set; } = [];
    public int YearMin { get; set; }
    public int YearMax { get; set; }
}

public sealed class RestaurantCatalogOptions
{
    public List<string> Cities { get; set; } = [];
    public List<string> Districts { get; set; } = [];
    public Dictionary<string, List<string>> DistrictsByCity { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Cuisines { get; set; } = [];
}

public sealed class RecipeCatalogOptions
{
    public List<string> Cuisines { get; set; } = [];
    public List<string> FoodTypes { get; set; } = [];
    public List<string> Ingredients { get; set; } = [];
    public int ComplexityMin { get; set; }
    public int ComplexityMax { get; set; }
    public int BudgetMin { get; set; }
    public int BudgetMax { get; set; }
}

public sealed class BoardGameCatalogOptions
{
    public List<string> GameTypes { get; set; } = [];
    public int PlayersMin { get; set; }
    public int PlayersMax { get; set; }
    public int DurationMin { get; set; }
    public int DurationMax { get; set; }
}

public sealed class SessionInvitationSummary
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string Category { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
    public string? InvitedByUserName { get; set; }
    public string? InvitedByDisplayName { get; set; }
    public string CategoryDisplayName => SwipeMateDisplayText.Category(Category);
    public string StatusDisplayName => SwipeMateDisplayText.Status(Status);
}

public sealed class SessionDetailsSummary
{
    public Guid Id { get; set; }
    public string Category { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
    public int SwipeCount { get; set; }
    public int MatchCount { get; set; }
    public int PendingInvitationCount { get; set; }
    public string? FiltersSummary { get; set; }
    public List<SessionParticipantSummary> Participants { get; set; } = [];
}

public sealed class SessionParticipantSummary
{
    public string Id { get; set; } = "";
    public string? UserName { get; set; }
    public string? DisplayName { get; set; }
}

public static class SwipeMateDisplayText
{
    public static string Category(string category)
        => category switch
        {
            "Movie" => "Филми и сериали",
            "Restaurant" => "Ресторанти",
            "Recipe" => "Рецепти",
            "BoardGame" => "Настолни игри",
            _ => category
        };

    public static string Status(string status)
        => status switch
        {
            "Active" => "Активна",
            "Pending" => "Чака покани",
            "Partial" => "Частично приета",
            "Finished" => "Приключена",
            "Closed" => "Приключена от създателя",
            "Expired" => "Изтекла",
            "Cancelled" => "Отменена",
            "Declined" => "Отказана",
            _ => status
        };

    public static string StatusColor(string status)
        => status switch
        {
            "Active" => "#C026D3",
            "Pending" => "#D97706",
            "Partial" => "#7C3AED",
            "Finished" => "#059669",
            "Closed" => "#6B7280",
            "Expired" => "#6B7280",
            "Cancelled" => "#6B7280",
            "Declined" => "#DC2626",
            _ => "#6B7280"
        };

    public static bool IsCurrentSessionStatus(string status)
        => string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase)
           || string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase)
           || string.Equals(status, "Partial", StringComparison.OrdinalIgnoreCase);
}

public sealed class AdminUserSummary
{
    public string Id { get; set; } = "";
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime? BlockedAtUtc { get; set; }
    public string? BlockedReason { get; set; }
    public int FriendsCount { get; set; }
    public int SessionsCount { get; set; }
    public int MatchesCount { get; set; }
    public List<string> Roles { get; set; } = [];
    public string RoleText => Roles.Count == 0 ? "User" : string.Join(", ", Roles);
    public string StatusText => IsBlocked ? "Блокиран" : "Активен";
    public string ActionText => IsBlocked ? "Отблокирай" : "Блокирай";
    public string BioPreview => string.IsNullOrWhiteSpace(Bio) ? "Няма описание." : Bio!;
    public string StatsText => $"Приятели: {FriendsCount} • Сесии: {SessionsCount} • Съвпадения: {MatchesCount}";
    public string AvatarInitials =>
        string.IsNullOrWhiteSpace(UserName)
            ? "SM"
            : string.Concat(UserName.Trim().Split(['_', '-', ' '], StringSplitOptions.RemoveEmptyEntries).Select(x => x[0]).Take(2)).ToUpperInvariant();
}

public sealed class AdminRecentSessionSummary
{
    public Guid Id { get; set; }
    public string Category { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
    public string Display => $"{SwipeMateDisplayText.Category(Category)} • {SwipeMateDisplayText.Status(Status)} • {CreatedAtUtc:dd.MM.yyyy HH:mm}";
}

public sealed class AdminRecentMatchSummary
{
    public Guid SessionId { get; set; }
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
    public string Display => $"{Title} • {SwipeMateDisplayText.Category(Category)} • {CreatedAtUtc:dd.MM.yyyy HH:mm}";
}

public sealed class AdminUserDetailsSummary
{
    public string Id { get; set; } = "";
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool IsBlocked { get; set; }
    public string? BlockedReason { get; set; }
    public List<string> Roles { get; set; } = [];
    public int FriendsCount { get; set; }
    public int SessionsCount { get; set; }
    public int MatchesCount { get; set; }
    public List<AdminRecentSessionSummary> RecentSessions { get; set; } = [];
    public List<AdminRecentMatchSummary> RecentMatches { get; set; } = [];
}

public sealed class AdminCatalogItemSummary
{
    public Guid Id { get; set; }
    public string Category { get; set; } = "";
    public string ExternalId { get; set; } = "";
    public string Title { get; set; } = "";
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? Summary { get; set; }
    public string StatusText => IsActive ? "Активно" : "Скрито";
    public string ActionText => IsActive ? "Деактивирай" : "Активирай";
}

public sealed class AdminCatalogItemDetailsSummary
{
    public Guid Id { get; set; }
    public string Category { get; set; } = "";
    public string ExternalId { get; set; } = "";
    public string Title { get; set; } = "";
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public string? SourceName { get; set; }
    public string? SourceUrl { get; set; }
    public string? MetaJson { get; set; }
}

public sealed class AvailableSuggestionCountResponse
{
    public int Count { get; set; }
}

