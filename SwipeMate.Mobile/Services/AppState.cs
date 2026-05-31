using System.Text;
using System.Text.Json;
using SwipeMate.Mobile.Models;

namespace SwipeMate.Mobile.Services;

public sealed class AppState
{
    private readonly ApiClient _apiClient;
    private readonly SemaphoreSlim _initializeLock = new(1, 1);
    private bool _isInitialized;

    public AppState(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public string? Token { get; private set; }
    public CurrentUser? User { get; private set; }
    public Guid? CurrentSessionId { get; set; }
    public string? CurrentCategory { get; set; }
    public bool CurrentSessionIsOwner { get; set; }
    public SessionItemSummary? CurrentMatch { get; set; }
    public string? CurrentMatchMessage { get; set; }
    public List<string> CurrentMatchedUsers { get; set; } = [];

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token) && User is not null;

    public void UpdateProfile(string? displayName, string? email)
    {
        if (User is null)
        {
            return;
        }

        User.DisplayName = displayName;
        User.Email = email;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        await _initializeLock.WaitAsync();
        try
        {
            if (_isInitialized)
            {
                return;
            }

            var storedToken = await SecureStorage.GetAsync("jwt");

            if (!string.IsNullOrWhiteSpace(storedToken) && IsJwtExpired(storedToken))
            {
                SecureStorage.Remove("jwt");
                storedToken = null;
            }

            if (string.IsNullOrWhiteSpace(Token))
            {
                var parsedUser = string.IsNullOrWhiteSpace(storedToken)
                    ? null
                    : ParseUserFromJwt(storedToken);

                if (parsedUser is null)
                {
                    Token = null;
                    User = null;
                    _apiClient.SetBearer(null);
                    SecureStorage.Remove("jwt");
                }
                else
                {
                    Token = storedToken;
                    User = parsedUser;
                    _apiClient.SetBearer(Token);
                }
            }

            _isInitialized = true;
        }
        finally
        {
            _initializeLock.Release();
        }
    }

    public async Task SetAuthenticatedAsync(string token)
    {
        Token = token;
        User = ParseUserFromJwt(token);
        _apiClient.SetBearer(token);
        await SecureStorage.SetAsync("jwt", token);
    }

    public async Task LogoutAsync()
    {
        Token = null;
        User = null;
        CurrentSessionId = null;
        CurrentCategory = null;
        CurrentSessionIsOwner = false;
        CurrentMatch = null;
        CurrentMatchMessage = null;
        CurrentMatchedUsers = [];
        _apiClient.SetBearer(null);
        SecureStorage.Remove("jwt");
        await Task.CompletedTask;
    }

    private static CurrentUser? ParseUserFromJwt(string token)
    {
        try
        {
            var root = ReadJwtPayload(token);
            if (IsJwtExpired(root))
            {
                return null;
            }

            var roles = new List<string>();
            AppendRoles(root, roles, "role");
            AppendRoles(root, roles, "roles");
            AppendRoles(root, roles, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

            return new CurrentUser
            {
                UserName = root.TryGetProperty("unique_name", out var userName)
                    ? userName.GetString() ?? "User"
                    : "User",
                DisplayName = root.TryGetProperty("name", out var displayName)
                    ? displayName.GetString()
                    : null,
                Email = root.TryGetProperty("email", out var email)
                    ? email.GetString()
                    : null,
                Roles = roles.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            };
        }
        catch
        {
            return null;
        }
    }

    private static bool IsJwtExpired(string token)
    {
        try
        {
            return IsJwtExpired(ReadJwtPayload(token));
        }
        catch
        {
            return true;
        }
    }

    private static bool IsJwtExpired(JsonElement root)
    {
        if (!root.TryGetProperty("exp", out var expElement) || !expElement.TryGetInt64(out var expSeconds))
        {
            return false;
        }

        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expSeconds);
        return expiresAt <= DateTimeOffset.UtcNow.AddMinutes(1);
    }

    private static JsonElement ReadJwtPayload(string token)
    {
        var parts = token.Split('.');
        if (parts.Length < 2)
        {
            throw new FormatException("Invalid JWT token.");
        }

        var payload = parts[1]
            .Replace('-', '+')
            .Replace('_', '/');

        payload = payload.PadRight(payload.Length + ((4 - payload.Length % 4) % 4), '=');
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static void AppendRoles(JsonElement root, List<string> roles, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var roleElement))
        {
            return;
        }

        if (roleElement.ValueKind == JsonValueKind.String)
        {
            var role = roleElement.GetString();
            if (!string.IsNullOrWhiteSpace(role))
            {
                roles.Add(role);
            }
            return;
        }

        if (roleElement.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var item in roleElement.EnumerateArray())
        {
            var role = item.GetString();
            if (!string.IsNullOrWhiteSpace(role))
            {
                roles.Add(role);
            }
        }
    }
}
