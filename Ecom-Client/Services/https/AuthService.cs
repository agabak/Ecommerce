using Ecom_Client.Models;

namespace Ecom_Client.Services.https;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<string> LoginAsync(string username, string password, CancellationToken token = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Users/login", new { username, password }, token);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Login failed with status code {response.StatusCode}");
        }

        var json = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        return json?["token"] ?? throw new KeyNotFoundException("Token not found in response.");
    }

    public async Task<string> Register(RegisterRequest model, CancellationToken token = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Users", model, token);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadAsStringAsync()
            : throw new HttpRequestException($"Registration failed with status code {response.StatusCode}");
    }
}

