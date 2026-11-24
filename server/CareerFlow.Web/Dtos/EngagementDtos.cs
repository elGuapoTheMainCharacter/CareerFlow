namespace CareerFlow.Web.Dtos;

public record TalentDto(
    string Id, string DisplayName,
    string LatestCvFileName, DateTime LatestCvUploadedAtUtc, int CvCount);

public class InviteRequest
{
    public Guid JobOpportunityId { get; set; }
}

public record NotificationDto(long Id, string Message, string? Link, bool IsRead, DateTime CreatedAtUtc);
public record NotificationsDto(IReadOnlyList<NotificationDto> Items, int UnreadCount);

public record ProfileDto(bool IsDiscoverable);

public class SetVisibilityRequest
{
    public bool IsDiscoverable { get; set; }
}