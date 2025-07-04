namespace Ecommerce.Common.Models.Users;

public class User
{
    public Guid UserId { get; set; }
    public Guid AddressId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } =  string.Empty;
    public string FirstName { get; set; } = string.Empty;   
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
