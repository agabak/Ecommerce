using Ecom_AuthApi.Model.Dtos;
using Ecom_AuthApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecom_AuthApi.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UsersController(IUserService service, IJwtTokenService jwtTokenService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserAddressDto model, CancellationToken token = default)
    {
        if (model == null)
            return BadRequest("User data is required.");

        if (!await service.IsUserUniqueAsync(model.Username, model.Email, token))
            return Conflict("Username or email already exists.");

        return Ok(await service.CreateUser(model, token));
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto model, CancellationToken token = default)
    {
        if (model == null)
            return BadRequest("Login data is required.");

        var user = await service.GetUserWithAddressById(model.UserName, token);

        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            return Unauthorized("Invalid username or password.");

        var jwtToken = jwtTokenService.GenerateToken(user);

        if (string.IsNullOrEmpty(jwtToken))
            return StatusCode(500, "Error generating JWT token.");

        return Ok(new { token = jwtToken });
    }

    [HttpPut("{userId:Guid}")]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserDto model, CancellationToken token = default)
    {
        if (model == null)
            return BadRequest("User data is required.");

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim))
            return Unauthorized("User ID claim is missing.");

        if (!Guid.TryParse(userIdClaim, out var userIdFromClaim))
            return Unauthorized("Invalid user ID claim.");

        if (userIdFromClaim != userId)
            return Forbid("You are not allowed to update another user's data.");

        var updatedUser = await service.UpdateUser(userId, model, token);
        return Ok(updatedUser);
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> DeleteUser(Guid userId, CancellationToken token = default)
    {
        var deleted = await service.DeleteUser(userId, token);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }
}
