using Ecom_AuthApi.Model.Dtos;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ecom_AuthApi.Services
{
    public class JwtTokenService(IConfiguration config) : IJwtTokenService
    {
        private readonly string _issuer = config["Jwt:Issuer"]!;
        private readonly string _audience = config["Jwt:Audience"]!;
        private readonly SymmetricSecurityKey _key = new(
            Encoding.UTF8.GetBytes(config["Jwt:Key"]!)
        );

        public string GenerateToken(UserWithAddressDto user)
        {
            var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("Address", $"{user.Street}, {user.City}, {user.State}, {user.ZipCode}")
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

