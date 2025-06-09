using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace OLMS.Api.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Recipient email address cannot be null or empty.", nameof(to));

            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_config["EmailSettings:SenderEmail"]));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
                email.Body = new TextPart("plain") { Text = body };

                using var smtp = new SmtpClient();

                int port = int.TryParse(_config["EmailSettings:Port"], out var parsedPort)
                    ? parsedPort
                    : throw new InvalidOperationException("EmailSettings:Port is not a valid integer or is missing.");

                SecureSocketOptions options = port == 465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;

                var smtpServer = _config["EmailSettings:SmtpServer"];
                var username = _config["EmailSettings:Username"];
                var password = _config["EmailSettings:SenderPassword"];

                if (string.IsNullOrWhiteSpace(smtpServer) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    throw new InvalidOperationException("SMTP server, username, or password is not configured properly.");
                }

                await smtp.ConnectAsync(smtpServer, port, options);
                await smtp.AuthenticateAsync(username, password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Recipient}.", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient}. Subject: {Subject}", to, subject);
                throw new InvalidOperationException("Failed to send email. Please check SMTP configuration and network connectivity.", ex);
            }
        }
    }
}
