using CareerFlow.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace CareerFlow.Web.Dtos;

public class OpportunityUpsertRequest
{
    [Required, MaxLength(160)] public string Title { get; set; } = string.Empty;
    [MaxLength(4000)] public string? Description { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public DateOnly? ClosingDate { get; set; }
    public Guid CompanyId { get; set; }
}

public record OpportunityDto(
    Guid Id, string Title, EmploymentType EmploymentType,
    decimal? SalaryMin, decimal? SalaryMax, DateOnly? ClosingDate,
    Guid CompanyId, string CompanyName, bool IsClosed, int ApplicantCount);