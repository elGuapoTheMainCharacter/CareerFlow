using CareerFlow.Web.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CareerFlow.Web.Filters;

// Stops the two shared public demo accounts from changing anything it is applied to.
// Reading (GET) is allowed, so the demo still shows everything.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class BlockDemoAccountAttribute : ActionFilterAttribute
{
    private static readonly string[] DemoAccounts =
    [
        "demo.employer@careerflow.dev",
        "demo.seeker@careerflow.dev"
    ];

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var request = context.HttpContext.Request;
        var isRead = HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method);

        // The username is the email address (set at registration and in the seeder).
        var name = context.HttpContext.User.Identity?.Name;

        if (!isRead && name is not null &&
            DemoAccounts.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            context.Result = new ObjectResult(new ApiError(
                "This is a shared demo account, so this action is switched off. Create your own free account to try it."))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
