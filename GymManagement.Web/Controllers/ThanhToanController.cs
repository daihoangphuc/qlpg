using GymManagement.Web.Data.Models;
using GymManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers
{
    [Authorize]
    public class ThanhToanController : Controller
    {
        private readonly IThanhToanService _thanhToanService;
        private readonly IDangKyService _dangKyService;
        private readonly ILogger<ThanhToanController> _logger;

        public ThanhToanController(
            IThanhToanService thanhToanService,
            IDangKyService dangKyService,
            ILogger<ThanhToanController> logger)
        {
            _thanhToanService = thanhToanService;
            _dangKyService = dangKyService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var payments = await _thanhToanService.GetAllAsync();
                return View(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting payments");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách thanh toán.";
                return View(new List<ThanhToan>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var payment = await _thanhToanService.GetByIdAsync(id);
                if (payment == null)
                {
                    return NotFound();
                }
                return View(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting payment details for ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin thanh toán.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment(int registrationId, decimal amount, string method, string? note = null)
        {
            try
            {
                var payment = await _thanhToanService.CreatePaymentAsync(registrationId, amount, method, note);
                
                if (method == "VNPAY")
                {
                    var returnUrl = Url.Action("VnPayReturn", "ThanhToan", null, Request.Scheme);
                    var paymentUrl = await _thanhToanService.CreateVnPayUrlAsync(payment.ThanhToanId, returnUrl!);
                    return Redirect(paymentUrl);
                }
                else if (method == "CASH")
                {
                    // Process cash payment immediately
                    await _thanhToanService.ProcessCashPaymentAsync(payment.ThanhToanId);
                    return Json(new { success = true, message = "Thanh toán tiền mặt thành công!" });
                }
                
                return Json(new { success = true, message = "Tạo thanh toán thành công!", paymentId = payment.ThanhToanId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating payment");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo thanh toán." });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayReturn()
        {
            try
            {
                var vnpayData = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
                var result = await _thanhToanService.ProcessVnPayReturnAsync(vnpayData);
                
                if (result)
                {
                    TempData["SuccessMessage"] = "Thanh toán VNPay thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Thanh toán VNPay thất bại!";
                }
                
                return RedirectToAction("MyRegistrations", "DangKy");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing VNPay return");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý kết quả thanh toán.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProcessCashPayment(int paymentId)
        {
            try
            {
                var result = await _thanhToanService.ProcessCashPaymentAsync(paymentId);
                if (result)
                {
                    return Json(new { success = true, message = "Xử lý thanh toán tiền mặt thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể xử lý thanh toán." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing cash payment");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xử lý thanh toán." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RefundPayment(int paymentId, string reason)
        {
            try
            {
                var result = await _thanhToanService.RefundPaymentAsync(paymentId, reason);
                if (result)
                {
                    return Json(new { success = true, message = "Hoàn tiền thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể hoàn tiền." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while refunding payment");
                return Json(new { success = false, message = "Có lỗi xảy ra khi hoàn tiền." });
            }
        }

        public async Task<IActionResult> PendingPayments()
        {
            try
            {
                var payments = await _thanhToanService.GetPendingPaymentsAsync();
                return View(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting pending payments");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách thanh toán chờ xử lý.";
                return View(new List<ThanhToan>());
            }
        }

        public async Task<IActionResult> SuccessfulPayments()
        {
            try
            {
                var payments = await _thanhToanService.GetSuccessfulPaymentsAsync();
                return View(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting successful payments");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách thanh toán thành công.";
                return View(new List<ThanhToan>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPaymentsByRegistration(int registrationId)
        {
            try
            {
                var payments = await _thanhToanService.GetByRegistrationIdAsync(registrationId);
                return Json(payments.Select(p => new {
                    id = p.ThanhToanId,
                    amount = p.SoTien,
                    method = p.PhuongThuc,
                    status = p.TrangThai,
                    date = p.NgayThanhToan.ToString("dd/MM/yyyy HH:mm"),
                    note = p.GhiChu
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting payments by registration");
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRevenue(DateTime startDate, DateTime endDate)
        {
            try
            {
                var revenue = await _thanhToanService.GetTotalRevenueAsync(startDate, endDate);
                return Json(new { 
                    success = true, 
                    revenue = revenue,
                    formattedRevenue = revenue.ToString("N0") + " VNĐ"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting revenue");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tính doanh thu." });
            }
        }

        public IActionResult PaymentForm(int registrationId)
        {
            ViewBag.RegistrationId = registrationId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetRegistrationInfo(int registrationId)
        {
            try
            {
                var registration = await _dangKyService.GetByIdAsync(registrationId);
                if (registration == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đăng ký." });
                }

                return Json(new {
                    success = true,
                    memberName = $"{registration.NguoiDung?.Ho} {registration.NguoiDung?.Ten}",
                    packageName = registration.GoiTap?.TenGoi ?? registration.LopHoc?.TenLop,
                    startDate = registration.NgayBatDau.ToString("dd/MM/yyyy"),
                    endDate = registration.NgayKetThuc.ToString("dd/MM/yyyy"),
                    status = registration.TrangThai
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting registration info");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải thông tin đăng ký." });
            }
        }
    }
}
