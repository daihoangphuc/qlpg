using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;

namespace GymManagement.Web.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>() ?? new EmailSettings();
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            await SendEmailAsync(toEmail, "", subject, body);
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendBulkEmailAsync(IEnumerable<string> toEmails, string subject, string body)
        {
            var tasks = toEmails.Select(email => SendEmailAsync(email, subject, body));
            await Task.WhenAll(tasks);
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string memberName, string username, string tempPassword)
        {
            var subject = "Chào mừng bạn đến với Gym Management System";
            var body = $@"
                <html>
                <body>
                    <h2>Chào mừng {memberName}!</h2>
                    <p>Tài khoản của bạn đã được tạo thành công.</p>
                    <p><strong>Thông tin đăng nhập:</strong></p>
                    <ul>
                        <li>Tên đăng nhập: {username}</li>
                        <li>Mật khẩu tạm thời: {tempPassword}</li>
                    </ul>
                    <p>Vui lòng đăng nhập và đổi mật khẩu ngay lập tức.</p>
                    <p>Trân trọng,<br/>Đội ngũ Gym Management</p>
                </body>
                </html>";

            await SendEmailAsync(toEmail, memberName, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string memberName, string resetLink)
        {
            var subject = "Đặt lại mật khẩu - Gym Management System";
            var body = $@"
                <html>
                <body>
                    <h2>Đặt lại mật khẩu</h2>
                    <p>Xin chào {memberName},</p>
                    <p>Bạn đã yêu cầu đặt lại mật khẩu. Vui lòng click vào link bên dưới để đặt lại mật khẩu:</p>
                    <p><a href='{resetLink}'>Đặt lại mật khẩu</a></p>
                    <p>Link này sẽ hết hạn sau 24 giờ.</p>
                    <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                    <p>Trân trọng,<br/>Đội ngũ Gym Management</p>
                </body>
                </html>";

            await SendEmailAsync(toEmail, memberName, subject, body);
        }

        public async Task SendRegistrationConfirmationEmailAsync(string toEmail, string memberName, string packageName, DateTime expiryDate)
        {
            var subject = "Xác nhận đăng ký gói tập - Gym Management System";
            var body = $@"
                <html>
                <body>
                    <h2>Xác nhận đăng ký thành công</h2>
                    <p>Xin chào {memberName},</p>
                    <p>Bạn đã đăng ký thành công gói tập: <strong>{packageName}</strong></p>
                    <p>Thông tin gói tập:</p>
                    <ul>
                        <li>Tên gói: {packageName}</li>
                        <li>Ngày hết hạn: {expiryDate:dd/MM/yyyy}</li>
                    </ul>
                    <p>Cảm ơn bạn đã tin tưởng và sử dụng dịch vụ của chúng tôi!</p>
                    <p>Trân trọng,<br/>Đội ngũ Gym Management</p>
                </body>
                </html>";

            await SendEmailAsync(toEmail, memberName, subject, body);
        }

        public async Task SendPaymentConfirmationEmailAsync(string toEmail, string memberName, decimal amount, string paymentMethod)
        {
            var subject = "Xác nhận thanh toán - Gym Management System";
            var body = $@"
                <html>
                <body>
                    <h2>Xác nhận thanh toán thành công</h2>
                    <p>Xin chào {memberName},</p>
                    <p>Chúng tôi đã nhận được thanh toán của bạn với thông tin sau:</p>
                    <ul>
                        <li>Số tiền: {amount:N0} VNĐ</li>
                        <li>Phương thức: {paymentMethod}</li>
                        <li>Thời gian: {DateTime.Now:dd/MM/yyyy HH:mm}</li>
                    </ul>
                    <p>Cảm ơn bạn đã thanh toán!</p>
                    <p>Trân trọng,<br/>Đội ngũ Gym Management</p>
                </body>
                </html>";

            await SendEmailAsync(toEmail, memberName, subject, body);
        }
    }
}
