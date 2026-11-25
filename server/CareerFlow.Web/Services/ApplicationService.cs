using CareerFlow.Web.Data;
using CareerFlow.Web.Dtos;
using CareerFlow.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CareerFlow.Web.Services;

public class ApplicationService(AppDbContext db, ICurrentUserService currentUser)
{
    public async Task<PagedResult<ApplicationListItemDto>> SearchAsync(ApplicationQuery request)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 5, 50);

        var query = db.JobApplications.AsNoTracking()
            .Where(a => a.OwnerId == currentUser.UserId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(a =>
                EF.Functions.ILike(a.JobOpportunity!.Title, $"%{term}%") ||
                EF.Functions.ILike(a.JobOpportunity.Company!.Name, $"%{term}%"));
        }

        if (request.Status.HasValue)
            query = query.Where(a => a.Status == request.Status.Value);

        if (request.CompanyId.HasValue)
            query = query.Where(a => a.JobOpportunity!.CompanyId == request.CompanyId.Value);

        query = request.Sort switch
        {
            "updated-asc" => query.OrderBy(a => a.UpdatedAtUtc),
            "company" => query.OrderBy(a => a.JobOpportunity!.Company!.Name),
            "job" => query.OrderBy(a => a.JobOpportunity!.Title),
            _ => query.OrderByDescending(a => a.UpdatedAtUtc)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new ApplicationListItemDto(
                a.Id, a.JobOpportunity!.Title, a.JobOpportunity.Company!.Name,
                a.Status, a.AppliedDate, a.NextActionDate, a.UpdatedAtUtc))
            .ToListAsync();

        return new PagedResult<ApplicationListItemDto>(items, page, pageSize, total);
    }

    public async Task<ApplicationDetailDto?> GetAsync(Guid id) =>
        await db.JobApplications.AsNoTracking()
            .Where(a => a.Id == id && a.OwnerId == currentUser.UserId)
            .Select(a => new ApplicationDetailDto(
                a.Id, a.JobOpportunityId, a.JobOpportunity!.Title, a.JobOpportunity.Company!.Name,
                a.Status, a.AppliedDate, a.NextActionDate, a.SourceUrl,
                a.CoverLetter, a.ResumeDocumentId,
                a.ResumeDocument == null ? null : a.ResumeDocument.OriginalFileName,
                a.StatusHistory.OrderByDescending(h => h.ChangedAtUtc)
                    .Select(h => new StatusChangeDto(h.FromStatus, h.ToStatus, h.ChangedAtUtc)).ToList(),
                a.Notes.OrderByDescending(n => n.CreatedAtUtc)
                    .Select(n => new NoteDto(n.Id, n.Body, n.CreatedAtUtc)).ToList()))
            .SingleOrDefaultAsync();

    public async Task<ApplicationDetailDto> CreateAsync(ApplicationCreateRequest request)
{
    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    var job = await db.JobOpportunities.AsNoTracking()
        .Where(j => j.Id == request.JobOpportunityId
            && !j.IsClosed
            && (j.ClosingDate == null || j.ClosingDate >= today))
        .Select(j => new { j.Title, j.OwnerId })
        .SingleOrDefaultAsync();
    if (job is null) throw new ArgumentException("This job does not exist or has closed.");

    var alreadyApplied = await db.JobApplications.AnyAsync(a =>
        a.OwnerId == currentUser.UserId && a.JobOpportunityId == request.JobOpportunityId);
    if (alreadyApplied) throw new ArgumentException("You have already applied for this job.");

    if (request.ResumeDocumentId == Guid.Empty)
        throw new ArgumentException("Please choose a CV to send.");

    // The CV must belong to the person applying, never someone else's.
    var ownsResume = await db.ResumeDocuments.AnyAsync(d =>
        d.Id == request.ResumeDocumentId && d.OwnerId == currentUser.UserId);
    if (!ownsResume) throw new ArgumentException("That CV was not found.");

    var applicantName = await db.Users.AsNoTracking()
        .Where(u => u.Id == currentUser.UserId)
        .Select(u => u.DisplayName)
        .SingleAsync();

    var entity = new JobApplication
    {
        JobOpportunityId = request.JobOpportunityId,
        OwnerId = currentUser.UserId,
        Status = ApplicationStatus.Applied,
        AppliedDate = today,
        ResumeDocumentId = request.ResumeDocumentId,
        CoverLetter = request.CoverLetter?.Trim()
    };

    db.JobApplications.Add(entity);
    db.ApplicationStatusChanges.Add(new ApplicationStatusChange
    {
        JobApplicationId = entity.Id,
        FromStatus = ApplicationStatus.Saved,
        ToStatus = ApplicationStatus.Applied
    });

    // Saved in the same SaveChanges as the application: both exist or neither does.
    db.Notifications.Add(new Notification
    {
        UserId = job.OwnerId,
        Message = $"{applicantName} applied for {job.Title}.",
        Link = $"/applicants?job={request.JobOpportunityId}"
    });

    await db.SaveChangesAsync();

    return (await GetAsync(entity.Id))!;
}

    public async Task<bool> UpdateAsync(Guid id, ApplicationUpdateRequest request)
    {
        var entity = await db.JobApplications.SingleOrDefaultAsync(a =>
            a.Id == id && a.OwnerId == currentUser.UserId);
        if (entity is null) return false;

        entity.NextActionDate = request.NextActionDate;
        entity.SourceUrl = request.SourceUrl?.Trim();
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> WithdrawAsync(Guid id)
    {
        var entity = await db.JobApplications.SingleOrDefaultAsync(a =>
            a.Id == id && a.OwnerId == currentUser.UserId);
        if (entity is null) return false;

        if (entity.Status == ApplicationStatus.Withdrawn) return true;
        if (entity.Status is ApplicationStatus.Offer or ApplicationStatus.Rejected)
            throw new InvalidOperationException("This application has already been decided.");

        db.ApplicationStatusChanges.Add(new ApplicationStatusChange
        {
            JobApplicationId = entity.Id,
            FromStatus = entity.Status,
            ToStatus = ApplicationStatus.Withdrawn
        });
        entity.Status = ApplicationStatus.Withdrawn;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<NoteDto?> AddNoteAsync(Guid id, string body)
    {
        var exists = await db.JobApplications.AnyAsync(a =>
            a.Id == id && a.OwnerId == currentUser.UserId);
        if (!exists) return null;

        var note = new ApplicationNote { JobApplicationId = id, Body = body.Trim() };
        db.ApplicationNotes.Add(note);
        await db.SaveChangesAsync();
        return new NoteDto(note.Id, note.Body, note.CreatedAtUtc);
    }
}
