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
    public SessionItemSummary? CurrentMatch { get; set; }
    public string? CurrentMatchMessage { get; set; }
    public List<string> CurrentMatchedUsers { get; set; } = [];

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);

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

            if (string.IsNullOrWhiteSpace(Token))
            {
                Token = storedToken;
                _apiClient.SetBearer(Token);

                if (!string.IsNullOrWhiteSpace(Token))
                {
                    User = ParseUserFromJwt(Token);
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
            var parts = token.Split('.');
            if (parts.Length < 2)
            {
                return null;
            }

            var payload = parts[1]
                .Replace('-', '+')
                .Replace('_', '/');

            payload = payload.PadRight(payload.Length + ((4 - payload.Length % 4) % 4), '=');
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

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
