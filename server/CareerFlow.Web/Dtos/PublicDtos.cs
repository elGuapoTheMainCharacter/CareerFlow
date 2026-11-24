using CareerFlow.Web.Models;

namespace CareerFlow.Web.Dtos;

public record PublicJobDto(
    Guid Id,
    string Title,
    string CompanyName,
    string? Location,
    EmploymentType EmploymentType,
    decimal? SalaryMin,
    decimal? SalaryMax,
    DateOnly? ClosingDate,
    DateTime PostedAtUtc
    );
public record PublicJobDetailDto(
    Guid Id, string Title, string? Description,
    string CompanyName, string? CompanyWebsite, string? Location,
    EmploymentType EmploymentType, decimal? SalaryMin, decimal? SalaryMax,
    DateOnly? ClosingDate, DateTime PostedAtUtc);