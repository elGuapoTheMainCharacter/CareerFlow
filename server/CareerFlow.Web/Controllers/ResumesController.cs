using CareerFlow.Web.Dtos;
using CareerFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerFlow.Web.Controllers;

[Authorize(Roles = "JobSeeker"), ApiController, Route("api/resumes")]
public class ResumesController(ResumeStorageService storage) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await storage.ListAsync());

    // 6 MB request limit leaves room for the multipart envelope around a 5 MB file.
    [HttpPost]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        try { return Ok(await storage.SaveAsync(file)); }
        catch (ArgumentException ex) { return BadRequest(new ApiError(ex.Message)); }
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var result = await storage.GetOwnAsync(id);
        if (result is null) return NotFound();
        var (meta, path) = result.Value;
        return PhysicalFile(path, meta.ContentType, meta.OriginalFileName);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try { return await storage.DeleteAsync(id) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new ApiError(ex.Message)); }
    }
}