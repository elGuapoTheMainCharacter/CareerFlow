using System.ComponentModel.DataAnnotations;

namespace CareerFlow.Web.Models;

public class JobOpportunity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public DateOnly? ClosingDate { get; set; }

    // A closed job is hidden from the public and accepts no new applications.
    public bool IsClosed { get; set; }

    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    [Required]
    public string OwnerId { get; set; } = string.Empty;
    public ApplicationUser? Owner { get; set; }

    public List<JobApplication> Applications { get; set; } = [];
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}