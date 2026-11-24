using CareerFlow.Web.Dtos;
using CareerFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerFlow.Web.Controllers;

[Authorize(Roles = "Employer"), ApiController, Route("api/opportunities")]
public class OpportunitiesController(OpportunityService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await service.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create(OpportunityUpsertRequest request)
    {
        try { return Ok(await service.CreateAsync(request)); }
        catch (ArgumentException ex) { return BadRequest(new ApiError(ex.Message)); }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, OpportunityUpsertRequest request)
    {
        try { return await service.UpdateAsync(id, request) ? NoContent() : NotFound(); }
        catch (ArgumentException ex) { return BadRequest(new ApiError(ex.Message)); }
    }

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id) =>
        await service.SetClosedAsync(id, true) ? NoContent() : NotFound();

    [HttpPost("{id:guid}/reopen")]
    public async Task<IActionResult> Reopen(Guid id) =>
        await service.SetClosedAsync(id, false) ? NoContent() : NotFound();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try { return await service.DeleteAsync(id) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new ApiError(ex.Message)); }
    }
}