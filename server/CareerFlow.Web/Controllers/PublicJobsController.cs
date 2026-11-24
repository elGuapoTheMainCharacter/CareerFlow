using CareerFlow.Web.Data;
using CareerFlow.Web.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerFlow.Web.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/public/jobs")]
public class PublicJobsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PublicJobDto>>> GetLatest([FromQuery] int take = 6)
    {
        take = Math.Clamp(take, 1, 50);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var jobs = await db.JobOpportunities.AsNoTracking()
            .Where(j => !j.IsClosed && (j.ClosingDate == null || j.ClosingDate >= today))
            .OrderByDescending(j => j.CreatedAtUtc)
            .Take(take)
            .Select(j => new PublicJobDto(
                j.Id, j.Title, j.Company!.Name, j.Company.Location,
                j.EmploymentType, j.SalaryMin, j.SalaryMax,
                j.ClosingDate, j.CreatedAtUtc))
            .ToListAsync();

        return Ok(jobs);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PublicJobDetailDto>> Get(Guid id)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var job = await db.JobOpportunities.AsNoTracking()
            .Where(j => j.Id == id && !j.IsClosed && (j.ClosingDate == null || j.ClosingDate >= today))
            .Select(j => new PublicJobDetailDto(
                j.Id, j.Title, j.Description,
                j.Company!.Name, j.Company.Website, j.Company.Location,
                j.EmploymentType, j.SalaryMin, j.SalaryMax,
                j.ClosingDate, j.CreatedAtUtc))
            .SingleOrDefaultAsync();

        return job is null ? NotFound() : Ok(job);
    }
}