using CareerFlow.Web.Data;
using CareerFlow.Web.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CareerFlow.Web.Services;

public class NotificationService(AppDbContext db, ICurrentUserService currentUser)
{
    public async Task<NotificationsDto> ListAsync()
    {
        var items = await db.Notifications.AsNoTracking()
            .Where(n => n.UserId == currentUser.UserId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(30)
            .Select(n => new NotificationDto(n.Id, n.Message, n.Link, n.IsRead, n.CreatedAtUtc))
            .ToListAsync();

        return new NotificationsDto(items, await UnreadCountAsync());
    }

    public Task<int> UnreadCountAsync() =>
        db.Notifications.CountAsync(n => n.UserId == currentUser.UserId && !n.IsRead);

    public async Task<bool> MarkReadAsync(long id)
    {
        var notification = await db.Notifications.SingleOrDefaultAsync(n =>
            n.Id == id && n.UserId == currentUser.UserId);
        if (notification is null) return false;

        notification.IsRead = true;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task MarkAllReadAsync() =>
        await db.Notifications
            .Where(n => n.UserId == currentUser.UserId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
}