namespace Ecom_AuthApi.Model.Dtos
{
    public class CreateUserDto
    {
        // Address fields
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;

        // User fields
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;    // Should be FirstName if not a typo
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public record UpdateUserDto(
        string Username,
        string Email,
        string FirstName,
        string LastName,
        string Phone
    );

    public class UserAddressDto
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;

        // User fields
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;    // Should be FirstName if not a typo
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }


    public record UserDto(
        Guid UserId,
        Guid AddressId,
        string Username,
        string Email,
        string FirstName,
        string LastName,
        string Phone
    );

    public class UserWithAddressDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;

        public string Street { get; set; } = default!;
        public string City { get; set; } = default!;
        public string State { get; set; } = default!;
        public string ZipCode { get; set; } = default!;

        public List<string> Roles { get; set; } = new();
    }

    public record UserLoginDto(string UserName, string Password);
}
