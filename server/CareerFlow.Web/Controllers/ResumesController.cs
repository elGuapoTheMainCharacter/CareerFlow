using CareerFlow.Web.Dtos;
using CareerFlow.Web.Filters;
using CareerFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerFlow.Web.Controllers;

// [BlockDemoAccount] stops the shared demo seeker from uploading or deleting CVs.
// If CI later says it cannot find BlockDemoAccountAttribute, delete this line and the Filters using.
[BlockDemoAccount]
[Authorize(Roles = "JobSeeker"), ApiController, Route("api/resumes")]
public class ResumesController(ResumeStorageService storage) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await storage.ListAsync());

    // 3 MB request limit leaves room for the multipart envelope around a 2 MB file.
    [HttpPost]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        try { return Ok(await storage.SaveAsync(file)); }
        catch (ArgumentException ex) { return BadRequest(new ApiError(ex.Message)); }
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var file = await storage.GetOwnAsync(id);
        return file is null ? NotFound() : File(file.Content, file.ContentType, file.FileName);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try { return await storage.DeleteAsync(id) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new ApiError(ex.Message)); }
    }
}
