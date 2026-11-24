namespace CareerFlow.Web.Dtos;
public record StatusCountDto(string Status, int Count);
public record DashboardDto(
 int Companies, int Opportunities, int Applications,
 int UpcomingActions, IReadOnlyList<StatusCountDto> ByStatus);