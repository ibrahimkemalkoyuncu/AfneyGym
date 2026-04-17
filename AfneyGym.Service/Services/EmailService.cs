using System.Net;
using System.Net.Mail;
using AfneyGym.Common.DTOs;
using AfneyGym.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace AfneyGym.Service.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
        {
            Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
            EnableSsl = _emailSettings.EnableSsl
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(toEmail);

        await client.SendMailAsync(mailMessage);
    }

    public async Task SendLessonReminderAsync(string toEmail, string memberName, string lessonName, DateTime lessonTime)
    {
        var body = $@"
            <h2>Ders Hatırlatması</h2>
            <p>Merhaba {memberName},</p>
            <p><strong>{lessonName}</strong> dersiniz <strong>{lessonTime:dd.MM.yyyy HH:mm}</strong>'de başlayacak.</p>
            <p>Unutmayın ve zamanında gelin!</p>
            <hr>
            <p><small>Bu bir otomatik hatırlatma e-postasıdır.</small></p>
        ";

        await SendEmailAsync(toEmail, $"⏰ Ders Hatırlatması: {lessonName}", body);
    }
}

