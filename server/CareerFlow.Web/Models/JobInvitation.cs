namespace CareerFlow.Web.Models;

public class JobInvitation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string EmployerId { get; set; } = string.Empty;
    public ApplicationUser? Employer { get; set; }

    public string SeekerId { get; set; } = string.Empty;
    public ApplicationUser? Seeker { get; set; }

    public Guid JobOpportunityId { get; set; }
    public JobOpportunity? JobOpportunity { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}