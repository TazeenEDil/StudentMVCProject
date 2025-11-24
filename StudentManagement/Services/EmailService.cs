using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StudentManagement.Helpers;
using StudentManagement.Interfaces.Services;
using StudentManagement.Models;
using System.Net;
using System.Net.Mail;

namespace StudentManagement.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly IWebHostEnvironment _env; 

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger,
            IWebHostEnvironment env)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _env = env;  // tells you which environment you're running in
        }
        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            if (!await EmailValidator.IsRealEmailAsync(toEmail))
                throw new Exception("Invalid or non-existent email address.");

            await SendInternalAsync(toEmail, subject, htmlMessage);
        }

        public async Task SendCredentialsEmailAsync(string toEmail, string username, string password)
        {
            if (!await EmailValidator.IsRealEmailAsync(toEmail))
                throw new Exception("Invalid or non-existent email address.");

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
            catch (SmtpFailedRecipientException ex)
            {
                _logger.LogError(ex, "Invalid email address. Recipient does not exist.");
                throw new Exception("The email address does not exist.");
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP error while sending email.");
                throw new Exception("Unable to send email. Email may not exist.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email");
                throw;
            }
        }

    }
}

