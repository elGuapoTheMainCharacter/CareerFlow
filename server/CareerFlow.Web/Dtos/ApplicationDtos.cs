using CareerFlow.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace CareerFlow.Web.Dtos;

public class ApplicationCreateRequest
{
    public Guid JobOpportunityId { get; set; }
    public Guid ResumeDocumentId { get; set; }

    [MaxLength(3000)]
    public string? CoverLetter { get; set; }
}

// A seeker may only change reminders. Status belongs to the employer.
public class ApplicationUpdateRequest
{
    public DateOnly? NextActionDate { get; set; }

    [MaxLength(1000), Url]
    public string? SourceUrl { get; set; }
}

public class AddNoteRequest
{
    [Required, MaxLength(2000)]
    public string Body { get; set; } = string.Empty;
}

public record StatusChangeDto(ApplicationStatus From, ApplicationStatus To, DateTime ChangedAtUtc);
public record NoteDto(long Id, string Body, DateTime CreatedAtUtc);

public record ApplicationListItemDto(
    Guid Id, string JobTitle, string CompanyName,
    ApplicationStatus Status, DateOnly? AppliedDate,
    DateOnly? NextActionDate, DateTime UpdatedAtUtc);

public record ApplicationDetailDto(
    Guid Id, Guid JobOpportunityId, string JobTitle, string CompanyName,
    ApplicationStatus Status, DateOnly? AppliedDate, DateOnly? NextActionDate,
    string? SourceUrl, string? CoverLetter, Guid? ResumeDocumentId, string? ResumeFileName,
    IReadOnlyList<StatusChangeDto> StatusHistory,
    IReadOnlyList<NoteDto> Notes);