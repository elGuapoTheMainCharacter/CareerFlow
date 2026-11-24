using CareerFlow.Web.Dtos;
using CareerFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerFlow.Web.Controllers;

[Authorize(Roles = "JobSeeker"), ApiController, Route("api/applications")]
public class ApplicationsController(ApplicationService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] ApplicationQuery query) =>
        Ok(await service.SearchAsync(query));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var item = await service.GetAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ApplicationCreateRequest request)
    {
        try { return Ok(await service.CreateAsync(request)); }
        catch (ArgumentException ex) { return BadRequest(new ApiError(ex.Message)); }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ApplicationUpdateRequest request) =>
        await service.UpdateAsync(id, request) ? NoContent() : NotFound();

    [HttpPost("{id:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid id)
    {
        try { return await service.WithdrawAsync(id) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new ApiError(ex.Message)); }
    }

    [HttpPost("{id:guid}/notes")]
    public async Task<IActionResult> AddNote(Guid id, AddNoteRequest request)
    {
        var note = await service.AddNoteAsync(id, request.Body);
        return note is null ? NotFound() : Ok(note);
    }
}