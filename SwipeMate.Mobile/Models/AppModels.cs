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
    public DateTime CreatedAtUtc { get; set; }
    public int ParticipantCount { get; set; }
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

