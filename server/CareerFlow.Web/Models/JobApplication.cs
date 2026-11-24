using System.ComponentModel.DataAnnotations;
namespace CareerFlow.Web.Models;
public class JobApplication
{
 public Guid Id { get; set; } = Guid.NewGuid();
 public Guid JobOpportunityId { get; set; }
 public JobOpportunity? JobOpportunity { get; set; }
 [Required]
 public string OwnerId { get; set; } = string.Empty;
 public ApplicationUser? Owner { get; set; }
 public ApplicationStatus Status { get; set; } = ApplicationStatus.Saved;
 public DateOnly? AppliedDate { get; set; }
 public DateOnly? NextActionDate { get; set; }
 [MaxLength(1000)]
 public string? SourceUrl { get; set; }
 public Guid? ResumeDocumentId { get; set; }
public ResumeDocument? ResumeDocument { get; set; }

[MaxLength(3000)]
public string? CoverLetter { get; set; }
 public List<ApplicationStatusChange> StatusHistory { get; set; } = [];
 public List<ApplicationNote> Notes { get; set; } = [];
 public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
 public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}