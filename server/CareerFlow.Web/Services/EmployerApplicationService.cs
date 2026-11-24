using CareerFlow.Web.Data;
using CareerFlow.Web.Dtos;
using CareerFlow.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CareerFlow.Web.Services;

public class EmployerApplicationService(AppDbContext db, ICurrentUserService currentUser)
{
    // Employers move people through the process. They cannot set Saved or Withdrawn.
    private static readonly ApplicationStatus[] EmployerSettable =
    [
        ApplicationStatus.Applied,
        ApplicationStatus.Assessment,
        ApplicationStatus.Interview,
        ApplicationStatus.Offer,
        ApplicationStatus.Rejected
    ];

    public async Task<IReadOnlyList<ReceivedApplicationDto>> ListAsync(Guid? jobId, ApplicationStatus? status)
    {
        // "Received" means the JOB belongs to me, not that I own the application row.
        var query = db.JobApplications.AsNoTracking()
            .Where(a => a.JobOpportunity!.OwnerId == currentUser.UserId);

        if (jobId.HasValue)
            query = query.Where(a => a.JobOpportunityId == jobId.Value);
        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return await query
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(200)
            .Select(a => new ReceivedApplicationDto(
                a.Id, a.JobOpportunityId, a.JobOpportunity!.Title, a.JobOpportunity.Company!.Name,
                a.Owner!.DisplayName, a.Owner.Email ?? "",
                a.Status, a.AppliedDate, a.CoverLetter,
                a.ResumeDocumentId != null,
                a.ResumeDocument == null ? null : a.ResumeDocument.OriginalFileName,
                a.UpdatedAtUtc))
            .ToListAsync();
    }

    public async Task<bool> SetStatusAsync(Guid id, ApplicationStatus newStatus)
    {
        if (!EmployerSettable.Contains(newStatus))
            throw new ArgumentException("You can set: Applied, Assessment, Interview, Accepted or Rejected.");

        var entity = await db.JobApplications
            .Include(a => a.JobOpportunity).ThenInclude(j => j!.Company)
            .SingleOrDefaultAsync(a => a.Id == id && a.JobOpportunity!.OwnerId == currentUser.UserId);
        if (entity is null) return false;

        if (entity.Status == ApplicationStatus.Withdrawn)
            throw new ArgumentException("The applicant withdrew this application.");

        // Only record a change when something actually changed.
        if (entity.Status != newStatus)
        {
            db.ApplicationStatusChanges.Add(new ApplicationStatusChange
            {
                JobApplicationId = entity.Id,
                FromStatus = entity.Status,
                ToStatus = newStatus
            });
            entity.Status = newStatus;
            entity.UpdatedAtUtc = DateTime.UtcNow;

            var message = MessageFor(newStatus, entity.JobOpportunity!.Company!.Name, entity.JobOpportunity.Title);
            if (message is not null)
            {
                db.Notifications.Add(new Notification
                {
                    UserId = entity.OwnerId,
                    Message = message,
                    Link = $"/applications/{entity.Id}"
                });
            }

            await db.SaveChangesAsync();
        }
        return true;
    }

    private static string? MessageFor(ApplicationStatus status, string company, string jobTitle) => status switch
    {
        ApplicationStatus.Assessment => $"{company} moved your application for {jobTitle} to the assessment stage.",
        ApplicationStatus.Interview => $"{company} would like to interview you for {jobTitle}.",
        ApplicationStatus.Offer => $"Congratulations! {company} accepted your application for {jobTitle}.",
        ApplicationStatus.Rejected => $"{company} did not move forward with your application for {jobTitle}.",
        _ => null
    };
}