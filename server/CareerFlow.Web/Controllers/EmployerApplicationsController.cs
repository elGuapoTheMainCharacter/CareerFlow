using CareerFlow.Web.Dtos;
using CareerFlow.Web.Models;
using CareerFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerFlow.Web.Controllers;

[Authorize(Roles = "Employer"), ApiController, Route("api/employer/applications")]
public class EmployerApplicationsController(
    EmployerApplicationService service,
    ResumeStorageService storage) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid? jobId, [FromQuery] ApplicationStatus? status) =>
        Ok(await service.ListAsync(jobId, status));

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid id, SetStatusRequest request)
    {
        try { return await service.SetStatusAsync(id, request.Status) ? NoContent() : NotFound(); }
        catch (ArgumentException ex) { return BadRequest(new ApiError(ex.Message)); }
    }

    [HttpGet("{id:guid}/resume")]
public async Task<IActionResult> Resume(Guid id)
{
    var file = await storage.GetForEmployerAsync(id);
    return file is null ? NotFound() : File(file.Content, file.ContentType, file.FileName);
}
}
