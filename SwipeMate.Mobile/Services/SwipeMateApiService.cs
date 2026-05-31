using System.Net.Http.Json;
using System.Text.Json;
using SwipeMate.Mobile.Models;

namespace SwipeMate.Mobile.Services;

public sealed class SwipeMateApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiClient _api;

    public SwipeMateApiService(ApiClient api)
    {
        _api = api;
    }

    public async Task<List<FriendSummary>> GetFriendsAsync(CancellationToken ct = default)
        => await GetAsync<List<FriendSummary>>("/api/friends", ct) ?? [];

    public async Task<List<FriendRequestSummary>> GetFriendRequestsAsync(CancellationToken ct = default)
        => await GetAsync<List<FriendRequestSummary>>("/api/friends/requests", ct) ?? [];

    public Task SendFriendRequestAsync(string userName, CancellationToken ct = default)
        => PostAsync("/api/friends/request", new { toUserName = userName }, ct);

    public Task RespondToRequestAsync(Guid requestId, bool accept, CancellationToken ct = default)
        => PostAsync("/api/friends/respond", new { requestId, accept }, ct);

    public async Task<List<FriendSummary>> SearchUsersAsync(string query, CancellationToken ct = default)
        => await GetAsync<List<FriendSummary>>($"/api/friends/search?q={Uri.EscapeDataString(query)}", ct) ?? [];

    public async Task<CreateSessionResponse> CreateSessionAsync(string category, IEnumerable<string> friendUserNames, CancellationToken ct = default)
        => await PostAsync<CreateSessionResponse>("/api/sessions", new
        {
            category,
            friendUserNames = friendUserNames.ToArray()
        }, ct) ?? throw new InvalidOperationException("Липсва отговор за сесията.");

    public Task SaveMovieFiltersAsync(Guid sessionId, string genres, string minRating, string yearFrom, string yearTo, CancellationToken ct = default)
        => PutAsync($"/api/sessions/{sessionId}/filters/movies", new
        {
            genres = SplitCsv(genres),
            minRating = ParseDouble(minRating),
            yearFrom = ParseInt(yearFrom),
            yearTo = ParseInt(yearTo)
        }, ct);

    public Task SaveRestaurantFiltersAsync(Guid sessionId, string city, string district, string cuisine, string minRating, CancellationToken ct = default)
        => PutAsync($"/api/sessions/{sessionId}/filters/restaurants", new
        {
            city = NullIfEmpty(city),
            district = NullIfEmpty(district),
            cuisine = NullIfEmpty(cuisine),
            minRating = ParseDouble(minRating)
        }, ct);

    public Task SaveRecipeFiltersAsync(Guid sessionId, string complexity, string cuisine, string foodType, string minRating, string ingredients, CancellationToken ct = default)
        => PutAsync($"/api/sessions/{sessionId}/filters/recipes", new
        {
            complexity = ParseInt(complexity),
            cuisine = NullIfEmpty(cuisine),
            foodType = NullIfEmpty(foodType),
            minRating = ParseDouble(minRating),
            ingredients = SplitCsv(ingredients)
        }, ct);

    public Task SaveBoardGameFiltersAsync(Guid sessionId, string gameType, string durationMin, string durationMax, string playersMin, string playersMax, string minRating, CancellationToken ct = default)
        => PutAsync($"/api/sessions/{sessionId}/filters/games", new
        {
            gameType = NullIfEmpty(gameType),
            durationMin = ParseInt(durationMin),
            durationMax = ParseInt(durationMax),
            playersMin = ParseInt(playersMin),
            playersMax = ParseInt(playersMax),
            minRating = ParseDouble(minRating)
        }, ct);

    public async Task<SessionItemSummary?> GetNextItemAsync(Guid sessionId, CancellationToken ct = default)
        => await GetAsync<SessionItemSummary>($"/api/sessions/{sessionId}/next", ct);

    public async Task<SwipeResponse> SwipeAsync(Guid sessionId, Guid itemId, bool isYes, CancellationToken ct = default)
        => await PostAsync<SwipeResponse>($"/api/sessions/{sessionId}/swipe", new { itemId, isYes }, ct)
           ?? new SwipeResponse { Ok = false };

    public async Task<List<SessionItemSummary>> GetSessionMatchesAsync(Guid sessionId, CancellationToken ct = default)
        => await GetAsync<List<SessionItemSummary>>($"/api/sessions/{sessionId}/matches", ct) ?? [];

    public async Task<List<SessionSummary>> GetMySessionsAsync(CancellationToken ct = default)
        => await GetAsync<List<SessionSummary>>("/api/sessions/mine", ct) ?? [];

    public async Task<SessionDetailsSummary> GetSessionDetailsAsync(Guid sessionId, CancellationToken ct = default)
        => await GetAsync<SessionDetailsSummary>($"/api/sessions/{sessionId}/details", ct)
           ?? throw new InvalidOperationException("Липсва отговор с детайли за сесията.");

    public async Task<List<SessionInvitationSummary>> GetSessionInvitationsAsync(CancellationToken ct = default)
        => await GetAsync<List<SessionInvitationSummary>>("/api/sessions/invitations", ct) ?? [];

    public Task RespondToSessionInvitationAsync(Guid invitationId, bool accept, CancellationToken ct = default)
        => PostAsync("/api/sessions/invitations/respond", new { invitationId, accept }, ct);

    public async Task<List<SessionItemSummary>> GetMyMatchesAsync(CancellationToken ct = default)
        => await GetAsync<List<SessionItemSummary>>("/api/sessions/mine/matches", ct) ?? [];

    public Task CloseSessionAsync(Guid sessionId, bool close = true, CancellationToken ct = default)
        => PostAsync($"/api/sessions/{sessionId}/close", new { close }, ct);

    public async Task<int> GetAvailableSuggestionCountAsync(Guid sessionId, CancellationToken ct = default)
        => (await GetAsync<AvailableSuggestionCountResponse>($"/api/sessions/{sessionId}/available-count", ct))?.Count ?? 0;

    public async Task<ProfileSummary> GetProfileAsync(CancellationToken ct = default)
        => await GetAsync<ProfileSummary>("/api/profile/me", ct)
           ?? throw new InvalidOperationException("Липсва отговор за профила.");

    public async Task<CatalogOptionsSummary> GetCatalogOptionsAsync(CancellationToken ct = default)
        => await GetAsync<CatalogOptionsSummary>("/api/catalog/options", ct)
           ?? new CatalogOptionsSummary();

    public async Task<ProfileSummary> UpdateProfileAsync(string displayName, string bio, string profileImageUrl, CancellationToken ct = default)
        => await PutAsync<ProfileSummary>("/api/profile/me", new
        {
            displayName = NullIfEmpty(displayName),
            bio = NullIfEmpty(bio),
            profileImageUrl = NullIfEmpty(profileImageUrl)
        }, ct) ?? throw new InvalidOperationException("Липсва отговор за профила.");

    public async Task<List<AdminUserSummary>> GetAdminUsersAsync(CancellationToken ct = default)
        => await GetAsync<List<AdminUserSummary>>("/api/admin/users", ct) ?? [];

    public async Task<AdminUserDetailsSummary> GetAdminUserDetailsAsync(string userId, CancellationToken ct = default)
        => await GetAsync<AdminUserDetailsSummary>($"/api/admin/users/{Uri.EscapeDataString(userId)}", ct)
           ?? throw new InvalidOperationException("Липсват детайли за потребителя.");

    public Task BlockAdminUserAsync(string userId, string? reason, CancellationToken ct = default)
        => PostAsync($"/api/admin/users/{Uri.EscapeDataString(userId)}/block", new { reason = NullIfEmpty(reason) }, ct);

    public Task UnblockAdminUserAsync(string userId, CancellationToken ct = default)
        => PostAsync($"/api/admin/users/{Uri.EscapeDataString(userId)}/unblock", new { }, ct);

    public async Task<List<AdminCatalogItemSummary>> GetAdminCatalogAsync(CancellationToken ct = default)
        => await GetAsync<List<AdminCatalogItemSummary>>("/api/admin/catalog", ct) ?? [];

    public async Task<AdminCatalogItemDetailsSummary> GetAdminCatalogDetailsAsync(Guid itemId, CancellationToken ct = default)
        => await GetAsync<AdminCatalogItemDetailsSummary>($"/api/admin/catalog/{itemId}", ct)
           ?? throw new InvalidOperationException("Липсват детайли за елемента от каталога.");

    public Task SetAdminCatalogStatusAsync(Guid itemId, bool isActive, CancellationToken ct = default)
        => PostAsync($"/api/admin/catalog/{itemId}/status", new { isActive }, ct);

    public async Task<string> GetAdminBackupJsonAsync(CancellationToken ct = default)
    {
        await _api.EnsureBearerAsync();
        using var response = await _api.Http.GetAsync("/api/admin/backup", ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadAsStringAsync(ct);
    }

    private async Task<T?> GetAsync<T>(string url, CancellationToken ct)
    {
        await _api.EnsureBearerAsync();
        using var response = await _api.Http.GetAsync(url, ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return default;
        }

        await EnsureSuccessAsync(response, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(body) || body == "null")
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(body, JsonOptions);
    }

    private async Task<T?> PostAsync<T>(string url, object body, CancellationToken ct)
    {
        await _api.EnsureBearerAsync();
        using var response = await _api.Http.PostAsJsonAsync(url, body, JsonOptions, ct);
        await EnsureSuccessAsync(response, ct);
        var payload = await response.Content.ReadAsStringAsync(ct);
        return string.IsNullOrWhiteSpace(payload) ? default : JsonSerializer.Deserialize<T>(payload, JsonOptions);
    }

    private async Task PostAsync(string url, object body, CancellationToken ct)
    {
        await _api.EnsureBearerAsync();
        using var response = await _api.Http.PostAsJsonAsync(url, body, JsonOptions, ct);
        await EnsureSuccessAsync(response, ct);
    }

    private async Task PutAsync(string url, object body, CancellationToken ct)
    {
        await _api.EnsureBearerAsync();
        using var response = await _api.Http.PutAsJsonAsync(url, body, JsonOptions, ct);
        await EnsureSuccessAsync(response, ct);
    }

    private async Task<T?> PutAsync<T>(string url, object body, CancellationToken ct)
    {
        await _api.EnsureBearerAsync();
        using var response = await _api.Http.PutAsJsonAsync(url, body, JsonOptions, ct);
        await EnsureSuccessAsync(response, ct);
        var payload = await response.Content.ReadAsStringAsync(ct);
        return string.IsNullOrWhiteSpace(payload) ? default : JsonSerializer.Deserialize<T>(payload, JsonOptions);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new HttpRequestException("Неоторизиран достъп. Моля, влез отново.");
        }

        var body = await response.Content.ReadAsStringAsync(ct);
        throw new HttpRequestException(string.IsNullOrWhiteSpace(body) ? response.ReasonPhrase : body);
    }

    private static string? NullIfEmpty(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int? ParseInt(string? value)
        => int.TryParse(value, out var result) ? result : null;

    private static double? ParseDouble(string? value)
        => double.TryParse(value, out var result) ? result : null;

    private static string[] SplitCsv(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
