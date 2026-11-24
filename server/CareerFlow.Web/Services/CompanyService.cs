using CareerFlow.Web.Data;
using CareerFlow.Web.Dtos;
using CareerFlow.Web.Models;
using Microsoft.EntityFrameworkCore;
namespace CareerFlow.Web.Services;
public class CompanyService(AppDbContext db, ICurrentUserService currentUser) : ICompanyService
{
 public async Task<IReadOnlyList<CompanyDto>> GetAllAsync() =>
 await db.Companies.AsNoTracking()
 .Where(c => c.OwnerId == currentUser.UserId)
 .OrderBy(c => c.Name)
 .Select(c => new CompanyDto(c.Id, c.Name, c.Website, c.Location, c.Opportunities.Count))
 .ToListAsync();
 public async Task<CompanyDto?> GetByIdAsync(Guid id) =>
 await db.Companies.AsNoTracking()
 .Where(c => c.Id == id && c.OwnerId == currentUser.UserId)
 .Select(c => new CompanyDto(c.Id, c.Name, c.Website, c.Location, c.Opportunities.Count))
 .SingleOrDefaultAsync();
 public async Task<CompanyDto> CreateAsync(CompanyUpsertRequest request)
 {
 var company = new Company
 {
 Name = request.Name.Trim(), Website = request.Website?.Trim(),
 Location = request.Location?.Trim(), OwnerId = currentUser.UserId
 };
 db.Companies.Add(company);
 await db.SaveChangesAsync();
 return new CompanyDto(company.Id, company.Name, company.Website, company.Location, 0);
 }
 public async Task<bool> UpdateAsync(Guid id, CompanyUpsertRequest request)
 {
 var company = await db.Companies.SingleOrDefaultAsync(c =>
 c.Id == id && c.OwnerId == currentUser.UserId);
 if (company is null) return false;
 company.Name = request.Name.Trim();
 company.Website = request.Website?.Trim();
 company.Location = request.Location?.Trim();
 await db.SaveChangesAsync();
 return true;
 }
 public async Task<bool> DeleteAsync(Guid id)
 {
 var company = await db.Companies
 .Include(c => c.Opportunities)
 .SingleOrDefaultAsync(c => c.Id == id && c.OwnerId == currentUser.UserId);
 if (company is null) return false;
 if (company.Opportunities.Count > 0)
 throw new InvalidOperationException("Delete the company's opportunities first.");
 db.Companies.Remove(company);
 await db.SaveChangesAsync();
 return true;
 }
}