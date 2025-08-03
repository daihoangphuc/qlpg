using Microsoft.AspNetCore.Mvc;
using GymManagement.Web.Services;

namespace GymManagement.Web.Controllers
{
    public class TestEmailController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<TestEmailController> _logger;

        public TestEmailController(IEmailService emailService, ILogger<TestEmailController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendTestEmail(string toEmail, string testType)
        {
            try
            {
                switch (testType)
                {
                    case "welcome":
                        await _emailService.SendWelcomeEmailAsync(toEmail, "Nguyễn Văn Test", "testuser", "TempPass123!");
                        break;
                    case "payment":
                        await _emailService.SendPaymentConfirmationEmailAsync(toEmail, "Nguyễn Văn Test", 500000, "VNPay");
                        break;
                    case "booking":
                        await _emailService.SendBookingConfirmationEmailAsync(toEmail, "Nguyễn Văn Test", "Yoga cơ bản", DateTime.Now.AddDays(1), "HLV Nguyễn Văn A");
                        break;
                    case "reminder":
                        await _emailService.SendClassReminderEmailAsync(toEmail, "Nguyễn Văn Test", "Yoga cơ bản", DateTime.Now.AddHours(2), "HLV Nguyễn Văn A", "Phòng tập 1");
                        break;
                    case "expiry":
                        await _emailService.SendMembershipExpiryReminderAsync(toEmail, "Nguyễn Văn Test", "Gói 3 tháng", DateTime.Now.AddDays(5), 5);
                        break;
                    default:
                        await _emailService.SendEmailAsync(toEmail, "Test Email", "<h2>Đây là email test</h2><p>Email service hoạt động bình thường!</p>");
                        break;
                }

                TempData["Success"] = $"Email test '{testType}' đã được gửi thành công đến {toEmail}";
                _logger.LogInformation("Test email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi gửi email: {ex.Message}";
                _logger.LogError(ex, "Failed to send test email to {Email}", toEmail);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}