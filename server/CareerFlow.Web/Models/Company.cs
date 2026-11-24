using System.ComponentModel.DataAnnotations;
namespace CareerFlow.Web.Models;
public class Company
{
 public Guid Id { get; set; } = Guid.NewGuid();
 [Required, MaxLength(120)]
 public string Name { get; set; } = string.Empty;
 [MaxLength(200)]
 public string? Website { get; set; }
 [MaxLength(120)]
 public string? Location { get; set; }
 [Required]
 public string OwnerId { get; set; } = string.Empty;
 public ApplicationUser? Owner { get; set; }
 public List<JobOpportunity> Opportunities { get; set; } = [];
 public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
public bool IsClosed { get; set; }
}
