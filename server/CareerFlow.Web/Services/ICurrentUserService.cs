namespace CareerFlow.Web.Services;
public interface ICurrentUserService
{
 string UserId { get; }
 bool IsAuthenticated { get; }
}
