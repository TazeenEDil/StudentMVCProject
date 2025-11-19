namespace StudentManagement.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
        Task SendCredentialsEmailAsync(string toEmail, string username, string password);
    }
}
