using CareerFlow.Web.Dtos;
namespace CareerFlow.Web.Services;
public interface ICompanyService
{
 Task<IReadOnlyList<CompanyDto>> GetAllAsync();
 Task<CompanyDto?> GetByIdAsync(Guid id);
 Task<CompanyDto> CreateAsync(CompanyUpsertRequest request);
 Task<bool> UpdateAsync(Guid id, CompanyUpsertRequest request);
 Task<bool> DeleteAsync(Guid id);
}