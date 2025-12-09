using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using TalentosPlus.Domain.Interfaces;

namespace TalentosPlus.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpServer = _configuration["EmailSettings:SmtpServer"];
        var port = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
        var senderEmail = _configuration["EmailSettings:SenderEmail"];
        var password = _configuration["EmailSettings:Password"];

        if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(password))
        {
            // Fail gracefully or log if not configured, but requirement says "No simulation", so we attempt it.
            // If env vars are missing, we can throw or just return.
             throw new Exception("Email configuration is missing");
        }

        using var client = new SmtpClient(smtpServer, port)
        {
            Credentials = new NetworkCredential(senderEmail, password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(to);

        await client.SendMailAsync(mailMessage);
    }
}
