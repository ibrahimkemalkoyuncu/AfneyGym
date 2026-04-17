using AfneyGym.Domain.Interfaces;
using AfneyGym.Service.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AfneyGym.Service.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToUserAsync(Guid userId, string title, string body, string category = "general")
    {
        await _hubContext.Clients.Group($"user:{userId}").SendAsync("Notify", new
        {
            title,
            body,
            category,
            at = DateTime.UtcNow
        });
    }
}

