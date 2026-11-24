using CareerFlow.Web.Models;

namespace CareerFlow.Web.Dtos;

public record ReceivedApplicationDto(
    Guid Id, Guid JobOpportunityId, string JobTitle, string CompanyName,
    string ApplicantName, string ApplicantEmail,
    ApplicationStatus Status, DateOnly? AppliedDate, string? CoverLetter,
    bool HasResume, string? ResumeFileName, DateTime UpdatedAtUtc);

public class SetStatusRequest
{
    public ApplicationStatus Status { get; set; }
}