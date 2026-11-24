using Microsoft.AspNetCore.Identity;
namespace CareerFlow.Web.Models;
public class ApplicationUser : IdentityUser
{
 public string DisplayName { get; set; } = string.Empty;
 public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
public bool IsDiscoverable { get; set; }
}
