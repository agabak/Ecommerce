using Ecom_Client.Models;

namespace Ecom_Client.Services.https
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string username, string password, CancellationToken token = default);
        Task<string> Register(RegisterRequest model, CancellationToken token = default);
    }
}