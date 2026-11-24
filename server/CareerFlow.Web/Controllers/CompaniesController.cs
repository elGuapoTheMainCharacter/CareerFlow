using CareerFlow.Web.Dtos;
using CareerFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerFlow.Web.Controllers;

[Authorize(Roles = "Employer")]
[ApiController]
[Route("api/companies")]
public class CompaniesController(ICompanyService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CompanyDto>>> GetAll() =>
        Ok(await service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CompanyDto>> Get(Guid id)
    {
        var company = await service.GetByIdAsync(id);
        return company is null ? NotFound() : Ok(company);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyDto>> Create(CompanyUpsertRequest request)
    {
        try
        {
            var company = await service.CreateAsync(request);
            return CreatedAtAction(nameof(Get), new { id = company.Id }, company);
        }
        catch (ArgumentException ex) { return BadRequest(new ApiError(ex.Message)); }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CompanyUpsertRequest request)
    {
        try { return await service.UpdateAsync(id, request) ? NoContent() : NotFound(); }
        catch (ArgumentException ex) { return BadRequest(new ApiError(ex.Message)); }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try { return await service.DeleteAsync(id) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new ApiError(ex.Message)); }
    }
}