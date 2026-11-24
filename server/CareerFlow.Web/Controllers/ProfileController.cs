using CareerFlow.Web.Data;
using CareerFlow.Web.Dtos;
using CareerFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerFlow.Web.Controllers;

[Authorize(Roles = "JobSeeker"), ApiController, Route("api/profile")]
public class ProfileController(AppDbContext db, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProfileDto>> Get()
    {
        var visible = await db.Users.AsNoTracking()
            .Where(u => u.Id == currentUser.UserId)
            .Select(u => u.IsDiscoverable)
            .SingleAsync();
        return Ok(new ProfileDto(visible));
    }

    [HttpPut("visibility")]
    public async Task<IActionResult> SetVisibility(SetVisibilityRequest request)
    {
        var user = await db.Users.SingleAsync(u => u.Id == currentUser.UserId);
        user.IsDiscoverable = request.IsDiscoverable;
        await db.SaveChangesAsync();
        return NoContent();
    }
}