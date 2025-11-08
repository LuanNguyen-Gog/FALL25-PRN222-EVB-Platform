using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Services.Interface;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _config;

    private readonly string _smtpServer;
    private readonly int _port;
    private readonly string _senderEmail;
    private readonly string _password;
    private readonly bool _enableSsl;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;

        // đọc config đúng key
        _smtpServer = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
        _port = int.Parse(_config["EmailSettings:Port"] ?? "587");
        _senderEmail = _config["EmailSettings:SenderEmail"] ?? throw new Exception("Missing EmailSettings:SenderEmail");
        _password = _config["EmailSettings:Password"] ?? throw new Exception("Missing EmailSettings:Password");
        _enableSsl = bool.TryParse(_config["EmailSettings:EnableSsl"], out var ssl) ? ssl : true;
    }

    public async Task SendEmailAsync(string recipientEmail, string subject, string body)
    {
        try
        {
            using var smtpClient = new SmtpClient(_smtpServer, _port)
            {
                Credentials = new NetworkCredential(_senderEmail, _password),
                EnableSsl = _enableSsl
            };
            smtpClient.UseDefaultCredentials = false;
            using var message = new MailMessage
            {
                From = new MailAddress(_senderEmail, "EVB Platform"),
                Subject = subject ?? "",
                Body = body ?? "",
                IsBodyHtml = true
            };
            message.To.Add(recipientEmail);
            _logger.LogInformation("SMTP sending to {To} via {Host}:{Port} ssl={Ssl}",
    recipientEmail, _smtpServer, _port, _enableSsl);
            await smtpClient.SendMailAsync(message);
            _logger.LogInformation("Email sent to {Recipient}", recipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient}", recipientEmail);
            throw;
        }
    }
}
