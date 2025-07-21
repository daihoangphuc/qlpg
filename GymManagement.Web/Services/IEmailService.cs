namespace GymManagement.Web.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendEmailAsync(string toEmail, string toName, string subject, string body);
        Task SendBulkEmailAsync(IEnumerable<string> toEmails, string subject, string body);
        Task SendWelcomeEmailAsync(string toEmail, string memberName, string username, string tempPassword);
        Task SendPasswordResetEmailAsync(string toEmail, string memberName, string resetLink);
        Task SendRegistrationConfirmationEmailAsync(string toEmail, string memberName, string packageName, DateTime expiryDate);
        Task SendPaymentConfirmationEmailAsync(string toEmail, string memberName, decimal amount, string paymentMethod);
    }
}
