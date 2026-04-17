namespace AfneyGym.Domain.Interfaces;

public interface INotificationService
{
    Task SendToUserAsync(Guid userId, string title, string body, string category = "general");
}

