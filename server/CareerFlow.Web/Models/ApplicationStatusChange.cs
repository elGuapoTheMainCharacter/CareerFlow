using System.ComponentModel.DataAnnotations;
namespace CareerFlow.Web.Models;
public class ApplicationStatusChange
{
 public long Id { get; set; }
 public Guid JobApplicationId { get; set; }
 public JobApplication? JobApplication { get; set; }
 public ApplicationStatus FromStatus { get; set; }
 public ApplicationStatus ToStatus { get; set; }
 public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
}