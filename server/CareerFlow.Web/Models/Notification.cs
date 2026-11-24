using System.ComponentModel.DataAnnotations;

namespace CareerFlow.Web.Models;

public class Notification
{
    public long Id { get; set; }

    // The recipient. Every notification query filters on this.
    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    [Required, MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    // An in-app path such as /applicants or /jobs/{id}.
    [MaxLength(200)]
    public string? Link { get; set; }

    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}