namespace CareerFlow.Web.Dtos;
public record ApiError(string Message, Dictionary<string, string[]>? Errors = null);
public record PagedResult<T>(
 IReadOnlyList<T> Items,
 int Page,
 int PageSize,
 int TotalCount)
{
 public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
