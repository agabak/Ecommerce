using System.Text.Json.Serialization;

namespace Ecom_Client.Models;

public class JwtUser
{
    [JsonPropertyName("sub")]
    public Guid UserId { get; set; }

    [JsonPropertyName("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;
}
