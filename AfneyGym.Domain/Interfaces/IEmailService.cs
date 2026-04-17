namespace AfneyGym.Domain.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
    Task SendLessonReminderAsync(string toEmail, string memberName, string lessonName, DateTime lessonTime);
}

