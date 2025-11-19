using StudentManagement.Interfaces.Services;
using Microsoft.Extensions.Options;
using StudentManagement.Models;
using System.Net;
using System.Net.Mail;

namespace StudentManagement.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
        {
            _emailSettings = options.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            await SendInternalAsync(toEmail, subject, htmlMessage);
        }

        public async Task SendCredentialsEmailAsync(string toEmail, string username, string password)
        {
            string html = $@"
                <h2>Your Login Credentials</h2>
                <p><strong>Email:</strong> {toEmail}</p>
                <p><strong>Username:</strong> {username}</p>
                <p><strong>Password:</strong> {password}</p>
                <br />
                <p>Please change your password after first login.</p>
            ";

            await SendInternalAsync(toEmail, "Your Account Credentials", html);
        }

        private async Task SendInternalAsync(string toEmail, string subject, string htmlMessage)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                    EnableSsl = true
                };

                var mail = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mail.To.Add(toEmail);

                await client.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email");
                throw;
            }
        }
    }
}
