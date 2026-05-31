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

        await EnsureSuccessAsync(resp, ct);

        var json = await resp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
        if (json?.Token is null) throw new Exception("РћС‚РіРѕРІРѕСЂСЉС‚ РїСЂРё РІС…РѕРґ РЅРµ СЃСЉРґСЉСЂР¶Р° С‚РѕРєРµРЅ.");
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

        await EnsureSuccessAsync(resp, ct);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(ct);
        if (!string.IsNullOrWhiteSpace(body))
        {
            throw new Exception(body);
        }

        response.EnsureSuccessStatusCode();
    }

    private sealed class LoginResponse
    {
        public string? Token { get; set; }
    }
}

