using System.ComponentModel.DataAnnotations;
namespace CareerFlow.Web.Models;
public class ApplicationNote
{
 public long Id { get; set; }
 public Guid JobApplicationId { get; set; }
 public JobApplication? JobApplication { get; set; }
 [Required, MaxLength(2000)]
 public string Body { get; set; } = string.Empty;
 public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
