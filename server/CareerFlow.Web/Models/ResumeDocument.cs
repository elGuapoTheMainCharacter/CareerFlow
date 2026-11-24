using System.ComponentModel.DataAnnotations;
namespace CareerFlow.Web.Models;
public class ResumeDocument
{
 public Guid Id { get; set; } = Guid.NewGuid();
 [Required] public string OwnerId { get; set; } = string.Empty;
 public ApplicationUser? Owner { get; set; }
 [Required, MaxLength(255)] public string OriginalFileName { get; set; } = string.Empty;
 [Required, MaxLength(255)] public string StoredFileName { get; set; } = string.Empty;
 [Required, MaxLength(100)] public string ContentType { get; set; } = string.Empty;
 public long SizeBytes { get; set; }
 public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}