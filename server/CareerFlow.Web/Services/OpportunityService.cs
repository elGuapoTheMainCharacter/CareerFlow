using CareerFlow.Web.Data;
using CareerFlow.Web.Dtos;
using CareerFlow.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CareerFlow.Web.Services;

public class OpportunityService(AppDbContext db, ICurrentUserService currentUser)
{
    public async Task<IReadOnlyList<OpportunityDto>> GetAllAsync() =>
        await db.JobOpportunities.AsNoTracking()
            .Where(j => j.OwnerId == currentUser.UserId)
            .OrderByDescending(j => j.CreatedAtUtc)
            .Select(j => new OpportunityDto(
                j.Id, j.Title, j.EmploymentType, j.SalaryMin, j.SalaryMax, j.ClosingDate,
                j.CompanyId, j.Company!.Name, j.IsClosed, j.Applications.Count))
            .ToListAsync();

    public async Task<OpportunityDto> CreateAsync(OpportunityUpsertRequest request)
    {
        await ValidateAsync(request);

        var entity = new JobOpportunity
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            EmploymentType = request.EmploymentType,
            SalaryMin = request.SalaryMin,
            SalaryMax = request.SalaryMax,
            ClosingDate = request.ClosingDate,
            CompanyId = request.CompanyId,
            OwnerId = currentUser.UserId
        };
        db.JobOpportunities.Add(entity);
        await db.SaveChangesAsync();

        var companyName = await db.Companies.Where(c => c.Id == entity.CompanyId)
            .Select(c => c.Name).SingleAsync();

        return new OpportunityDto(
            entity.Id, entity.Title, entity.EmploymentType, entity.SalaryMin, entity.SalaryMax,
            entity.ClosingDate, entity.CompanyId, companyName, entity.IsClosed, 0);
    }

    public async Task<bool> UpdateAsync(Guid id, OpportunityUpsertRequest request)
    {
        await ValidateAsync(request);

        var entity = await db.JobOpportunities.SingleOrDefaultAsync(j =>
            j.Id == id && j.OwnerId == currentUser.UserId);
        if (entity is null) return false;

        entity.Title = request.Title.Trim();
        entity.Description = request.Description?.Trim();
        entity.EmploymentType = request.EmploymentType;
        entity.SalaryMin = request.SalaryMin;
        entity.SalaryMax = request.SalaryMax;
        entity.ClosingDate = request.ClosingDate;
        entity.CompanyId = request.CompanyId;
        await db.SaveChangesAsync();
        return true;
    }

    // Closing hides the job from the public and blocks new applications.
    // Existing applications stay, and the employer can still review them.
    public async Task<bool> SetClosedAsync(Guid id, bool closed)
    {
        var entity = await db.JobOpportunities.SingleOrDefaultAsync(j =>
            j.Id == id && j.OwnerId == currentUser.UserId);
        if (entity is null) return false;

        entity.IsClosed = closed;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await db.JobOpportunities.Include(j => j.Applications)
            .SingleOrDefaultAsync(j => j.Id == id && j.OwnerId == currentUser.UserId);
        if (entity is null) return false;

        if (entity.Applications.Count > 0)
            throw new InvalidOperationException("This job has applicants, so it cannot be deleted. Close it instead.");

        db.JobOpportunities.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }

    private async Task ValidateAsync(OpportunityUpsertRequest request)
    {
        if (request.SalaryMin is < 0 || request.SalaryMax is < 0)
            throw new ArgumentException("Salary values cannot be negative.");
        if (request.SalaryMin.HasValue && request.SalaryMax.HasValue && request.SalaryMin > request.SalaryMax)
            throw new ArgumentException("Minimum salary cannot exceed maximum salary.");

        var ownsCompany = await db.Companies.AnyAsync(c =>
            c.Id == request.CompanyId && c.OwnerId == currentUser.UserId);
        if (!ownsCompany) throw new ArgumentException("Company does not exist for this user.");
    }
}