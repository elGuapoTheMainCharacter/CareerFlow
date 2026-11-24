using CareerFlow.Web.Dtos;
using CareerFlow.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CareerFlow.Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : ControllerBase
{
    private static readonly string[] SelfAssignableRoles = ["Employer", "JobSeeker"];

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Register(RegisterRequest request)
    {
        if (!SelfAssignableRoles.Contains(request.AccountType))
            return BadRequest(new ApiError("Please choose an account type: Employer or JobSeeker."));

        var user = new ApplicationUser
{
    UserName = request.Email.Trim().ToLowerInvariant(),
    Email = request.Email.Trim().ToLowerInvariant(),
    DisplayName = request.DisplayName.Trim(),
    // Only job seekers can be listed, whatever the client sends.
    IsDiscoverable = request.AccountType == "JobSeeker" && request.IsDiscoverable
};

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Description).ToArray());

            var message = string.Join(" ", result.Errors.Select(e => e.Description));
            return BadRequest(new ApiError(message, errors));
        }

        await userManager.AddToRoleAsync(user, request.AccountType);
        await signInManager.SignInAsync(user, isPersistent: false);
        return Ok(await ToDto(user));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null) return Unauthorized(new ApiError("Invalid email or password."));

        var result = await signInManager.PasswordSignInAsync(
            user, request.Password, request.RememberMe, lockoutOnFailure: true);

        if (result.IsLockedOut)
            return StatusCode(423, new ApiError("Account temporarily locked after repeated failed logins."));
        if (!result.Succeeded)
            return Unauthorized(new ApiError("Invalid email or password."));

        return Ok(await ToDto(user));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return Unauthorized();
        return Ok(await ToDto(user));
    }

    private async Task<UserDto> ToDto(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new UserDto(user.Id, user.DisplayName, user.Email!, roles.ToList());
    }
}