using System.Security.Claims;
namespace CareerFlow.Web.Services;
public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
 public bool IsAuthenticated => accessor.HttpContext?.User.Identity?.IsAuthenticated == true;
 public string UserId => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
 ?? throw new UnauthorizedAccessException("No authenticated user.");
}