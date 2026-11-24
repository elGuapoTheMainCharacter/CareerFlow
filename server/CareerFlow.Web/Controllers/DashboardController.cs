using CareerFlow.Web.Data;
using CareerFlow.Web.Dtos;
using CareerFlow.Web.Models;
using CareerFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerFlow.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController(AppDbContext db, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get()
    {
        var userId = currentUser.UserId;

        if (User.IsInRole("Employer"))
            return Ok(await BuildEmployerDashboard(userId));

        return Ok(await BuildSeekerDashboard(userId));
    }

    // Employer: my companies, my open postings, and applications people sent to MY jobs.
    private async Task<DashboardDto> BuildEmployerDashboard(string userId)
    {
        var companies = await db.Companies.CountAsync(c => c.OwnerId == userId);
        var opportunities = await db.JobOpportunities.CountAsync(j => j.OwnerId == userId);

        // "Received" = applications whose JOB belongs to me, not applications I own.
        var received = db.JobApplications.AsNoTracking()
            .Where(a => a.JobOpportunity!.OwnerId == userId);

        var applications = await received.CountAsync();
        var byStatus = await CountByStatus(received);

        return new DashboardDto(companies, opportunities, applications, 0, byStatus);
    }

    // Job seeker: the applications I made.
    private async Task<DashboardDto> BuildSeekerDashboard(string userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var nextWeek = today.AddDays(7);

        var mine = db.JobApplications.AsNoTracking().Where(a => a.OwnerId == userId);

        var applications = await mine.CountAsync();
        var upcoming = await mine.CountAsync(a =>
            a.NextActionDate != null && a.NextActionDate >= today && a.NextActionDate <= nextWeek);
        var byStatus = await CountByStatus(mine);

        return new DashboardDto(0, 0, applications, upcoming, byStatus);
    }

    // Shared helper: the database counts, C# finishes the shaping.
    private static async Task<List<StatusCountDto>> CountByStatus(IQueryable<JobApplication> source)
    {
        var counts = await source
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return counts
            .OrderByDescending(x => x.Count)
            .Select(x => new StatusCountDto(x.Status.ToString(), x.Count))
            .ToList();
    }
}