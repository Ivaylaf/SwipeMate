using System.Net.Http.Headers;

namespace SwipeMate.Mobile.Services;

public class ApiClient
{
    public const string ApiBaseUrlPreferenceKey = "api_base_url";

    private HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = CreateHttpClient(GetSavedOrDefaultBaseUrl());
    }

    public void SetBearer(string? token)
    {
        _http.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrWhiteSpace(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task EnsureBearerAsync()
    {
        var token = await SecureStorage.GetAsync("jwt");
        var current = _http.DefaultRequestHeaders.Authorization?.Parameter;

        if (string.IsNullOrWhiteSpace(token))
        {
            if (_http.DefaultRequestHeaders.Authorization is not null)
            {
                SetBearer(null);
            }

            return;
        }

        if (!string.Equals(current, token, StringComparison.Ordinal))
        {
            SetBearer(token);
        }
    }

    public string CurrentBaseUrl => _http.BaseAddress?.ToString()?.TrimEnd('/') ?? "";

    public void UpdateBaseUrl(string url)
    {
        var normalized = NormalizeBaseUrl(url);

        if (string.Equals(normalized, CurrentBaseUrl, StringComparison.OrdinalIgnoreCase))
        {
            Preferences.Set(ApiBaseUrlPreferenceKey, normalized);
            return;
        }

        var existingToken = _http.DefaultRequestHeaders.Authorization?.Parameter;
        _http.Dispose();
        _http = CreateHttpClient(normalized);
        SetBearer(existingToken);
        Preferences.Set(ApiBaseUrlPreferenceKey, normalized);
    }

    public static string GetSavedOrDefaultBaseUrl()
    {
        var saved = Preferences.Get(ApiBaseUrlPreferenceKey, string.Empty);
        return string.IsNullOrWhiteSpace(saved) ? GetDefaultBaseUrl() : NormalizeBaseUrl(saved);
    }

    private static string GetDefaultBaseUrl()
    {
#if ANDROID
        return "http://10.0.2.2:5274";
#else
        return "http://localhost:5274";
#endif
    }

    private static string NormalizeBaseUrl(string url)
    {
        var value = url.Trim();
        if (!value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            value = $"http://{value}";
        }

        return value.TrimEnd('/');
    }

    private static HttpClient CreateHttpClient(string baseUrl)
    {
        return new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    public HttpClient Http => _http;
}
