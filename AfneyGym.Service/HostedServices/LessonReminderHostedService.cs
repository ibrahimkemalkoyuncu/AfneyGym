using AfneyGym.Data.Context;
using AfneyGym.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AfneyGym.Service.HostedServices;

/// <summary>
/// Ders başlamadan 2 saat önce katılımcılara hatırlatma e-postası gönderen background servis.
/// </summary>
public class LessonReminderHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LessonReminderHostedService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(30); // Her 30 dakikada kontrol et

    public LessonReminderHostedService(IServiceProvider serviceProvider, ILogger<LessonReminderHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LessonReminderHostedService başlatıldı");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendLessonRemindersAsync(stoppingToken);
                await Task.Delay(CheckInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("LessonReminderHostedService iptal edildi");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LessonReminderHostedService'de hata oluştu");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task SendLessonRemindersAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        try
        {
            var now = DateTime.Now;
            var twoHoursFromNow = now.AddHours(2);

            // Başlamak üzere olan dersler (2 saat içinde)
            var upcomingLessons = await context.Lessons
                .Include(l => l.Attendees)
                .ThenInclude(la => la.User)
                .Include(l => l.Trainer)
                .Where(l => !l.IsDeleted &&
                           l.StartTime > now &&
                           l.StartTime <= twoHoursFromNow)
                .ToListAsync(cancellationToken);

            foreach (var lesson in upcomingLessons)
            {
                var hasChanges = false;
                foreach (var attendee in lesson.Attendees.Where(a => !a.IsDeleted && a.ReminderSentAt == null))
                {
                    try
                    {
                        await emailService.SendLessonReminderAsync(
                            attendee.User!.Email,
                            $"{attendee.User.FirstName} {attendee.User.LastName}",
                            lesson.Name,
                            lesson.StartTime);

                        await notificationService.SendToUserAsync(
                            attendee.UserId,
                            "Ders Hatirlatmasi",
                            $"{lesson.Name} dersi {lesson.StartTime:dd.MM.yyyy HH:mm} saatinde basliyor.",
                            "reminder");

                        attendee.ReminderSentAt = DateTime.UtcNow;
                        hasChanges = true;

                        _logger.LogInformation($"Ders hatırlatması gönderildi: {lesson.Name} -> {attendee.User.Email}");
                    }
                    catch (Exception ex)
                    {
                        var attendeeEmail = attendee.User?.Email ?? "unknown";
                        _logger.LogWarning(ex, $"Hatırlatma e-postası gönderilemedi: {attendeeEmail}");
                    }
                }

                if (hasChanges)
                    await context.SaveChangesAsync(cancellationToken);
            }

            if (upcomingLessons.Any())
                _logger.LogInformation($"{upcomingLessons.Count} ders için hatırlatma gönderme işlemi tamamlandı");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ders hatırlatma işlemi sırasında hata oluştu");
        }
    }
}

