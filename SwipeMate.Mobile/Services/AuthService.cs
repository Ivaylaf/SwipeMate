using System.Net.Http.Json;

namespace SwipeMate.Mobile.Services;

public class AuthService
{
    private readonly ApiClient _api;

    public AuthService(ApiClient api) => _api = api;

    public async Task<string> LoginAsync(string userNameOrEmail, string password, CancellationToken ct = default)
    {
        var resp = await _api.Http.PostAsJsonAsync("/api/auth/login", new
        {
            userNameOrEmail,
            password
        }, ct);

        resp.EnsureSuccessStatusCode();

        // Очакваме { token: "..." }
        var json = await resp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
        if (json?.Token is null) throw new Exception("Login response missing token.");
        return json.Token;
    }

    public async Task RegisterAsync(string userName, string email, string password, string displayName, CancellationToken ct = default)
    {
        var resp = await _api.Http.PostAsJsonAsync("/api/auth/register", new
        {
            userName,
            email,
            password,
            displayName
        }, ct);

        resp.EnsureSuccessStatusCode();
    }

    private sealed class LoginResponse
    {
        public string? Token { get; set; }
    }
}

