using CareerFlow.Web.Data;
using CareerFlow.Web.Dtos;
using CareerFlow.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CareerFlow.Web.Services;

public class TalentService(AppDbContext db, ICurrentUserService currentUser)
{
    private const int PageSize = 12;

    public async Task<PagedResult<TalentDto>> SearchAsync(string? search, int page)
    {
        page = Math.Max(1, page);

        // Only people who opted in AND have at least one CV.
        var query = db.Users.AsNoTracking()
            .Where(u => u.IsDiscoverable && db.ResumeDocuments.Any(r => r.OwnerId == u.Id));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u => EF.Functions.Like(u.DisplayName, $"%{term}%"));
        }

        var total = await query.CountAsync();

        var people = await query
            .OrderBy(u => u.DisplayName)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(u => new { u.Id, u.DisplayName })
            .ToListAsync();

        // One extra small query for this page's CVs, instead of a complicated join.
        var ids = people.Select(p => p.Id).ToList();
        var docs = await db.ResumeDocuments.AsNoTracking()
            .Where(r => ids.Contains(r.OwnerId))
            .ToListAsync();

        var items = people
            .Select(p =>
            {
                var mine = docs.Where(d => d.OwnerId == p.Id)
                    .OrderByDescending(d => d.UploadedAtUtc)
                    .ToList();
                return mine.Count == 0
                    ? null
                    : new TalentDto(p.Id, p.DisplayName, mine[0].OriginalFileName, mine[0].UploadedAtUtc, mine.Count);
            })
            .OfType<TalentDto>()
            .ToList();

        return new PagedResult<TalentDto>(items, page, PageSize, total);
    }

    // Returns false when the person does not exist or is not discoverable.
    public async Task<bool> InviteAsync(string seekerId, Guid jobId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var job = await db.JobOpportunities.Include(j => j.Company)
            .SingleOrDefaultAsync(j => j.Id == jobId && j.OwnerId == currentUser.UserId);
        if (job is null) throw new ArgumentException("Job not found.");

        if (job.IsClosed || (job.ClosingDate != null && job.ClosingDate < today))
            throw new ArgumentException("You can only invite people to an open job.");

        var visible = await db.Users.AnyAsync(u => u.Id == seekerId && u.IsDiscoverable);
        if (!visible) return false;

        if (await db.JobApplications.AnyAsync(a => a.OwnerId == seekerId && a.JobOpportunityId == jobId))
            throw new ArgumentException("This person has already applied to that job.");

        if (await db.JobInvitations.AnyAsync(i => i.SeekerId == seekerId && i.JobOpportunityId == jobId))
            throw new ArgumentException("You have already invited this person to that job.");

        db.JobInvitations.Add(new JobInvitation
        {
            EmployerId = currentUser.UserId,
            SeekerId = seekerId,
            JobOpportunityId = jobId
        });
        db.Notifications.Add(new Notification
        {
            UserId = seekerId,
            Message = $"{job.Company!.Name} invited you to apply for {job.Title}.",
            Link = $"/jobs/{job.Id}"
        });

        await db.SaveChangesAsync();
        return true;
    }
}