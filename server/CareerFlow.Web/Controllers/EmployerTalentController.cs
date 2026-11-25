using CareerFlow.Web.Dtos;
using CareerFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerFlow.Web.Controllers;

[Authorize(Roles = "Employer"), ApiController, Route("api/employer/talent")]
public class EmployerTalentController(TalentService talent, ResumeStorageService storage) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? search, [FromQuery] int page = 1) =>
        Ok(await talent.SearchAsync(search, page));

    [HttpGet("{userId}/cv")]
public async Task<IActionResult> Cv(string userId)
{
    var file = await storage.GetLatestForTalentAsync(userId);
    return file is null ? NotFound() : File(file.Content, file.ContentType, file.FileName);
}

    [HttpPost("{userId}/invite")]
    public async Task<IActionResult> Invite(string userId, InviteRequest request)
    {
        try { return await talent.InviteAsync(userId, request.JobOpportunityId) ? NoContent() : NotFound(); }
        catch (ArgumentException ex) { return BadRequest(new ApiError(ex.Message)); }
    }
}
