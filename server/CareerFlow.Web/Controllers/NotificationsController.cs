using CareerFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerFlow.Web.Controllers;

[Authorize, ApiController, Route("api/notifications")]
public class NotificationsController(NotificationService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await service.ListAsync());

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount() => Ok(new { count = await service.UnreadCountAsync() });

    [HttpPost("{id:long}/read")]
    public async Task<IActionResult> MarkRead(long id) =>
        await service.MarkReadAsync(id) ? NoContent() : NotFound();

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        await service.MarkAllReadAsync();
        return NoContent();
    }
}