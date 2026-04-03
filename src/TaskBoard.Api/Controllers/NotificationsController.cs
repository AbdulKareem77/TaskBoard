using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationsController(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userIdStr = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var (items, totalCount, unreadCount) = await _notificationRepository.GetByUserIdAsync(userId, unreadOnly, page, pageSize);

        return Ok(new
        {
            items,
            totalCount,
            unreadCount,
            page,
            pageSize
        });
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userIdStr = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        await _notificationRepository.MarkAsReadAsync(id, userId);
        return Ok(new { success = true });
    }
}
