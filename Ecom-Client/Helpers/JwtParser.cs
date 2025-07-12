using System.Text.Json;

namespace Ecom_Client.Helpers;

public static class JwtParser
{
    public static T? ParsePayload<T>(string jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
            return default;

        var parts = jwt.Split('.');
        if (parts.Length != 3)
            return default;

        var payload = parts[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);

        return JsonSerializer.Deserialize<T>(jsonBytes);
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}


