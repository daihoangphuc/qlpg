using Microsoft.AspNetCore.Mvc;
using GymManagement.Web.Data;
using GymManagement.Web.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Web.Areas.VNPayAPI.Controllers
{
    [Area("VNPayAPI")]
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly GymDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IConfiguration configuration, GymDbContext context, ILogger<HomeController> logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request)
        {
            try
            {
                // Validate payment record exists
                var payment = await _context.ThanhToans.FindAsync(request.ThanhToanId);
                if (payment == null)
                {
                    return Json(new { success = false, message = "Payment not found" });
                }

                // VNPay Configuration
                var vnpayConfig = _configuration.GetSection("VnPay");
                var vnp_Url = vnpayConfig["BaseUrl"];
                var vnp_TmnCode = vnpayConfig["TmnCode"];
                var vnp_HashSecret = vnpayConfig["HashSecret"];

                // Build VNPay payment URL
                var vnpay = new VnPayLibrary();

                // Required parameters
                vnpay.AddRequestData("vnp_Version", "2.1.0");
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
                vnpay.AddRequestData("vnp_Amount", ((long)(payment.SoTien * 100)).ToString());
                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(HttpContext));
                vnpay.AddRequestData("vnp_Locale", "vn");
                vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan gym {payment.ThanhToanId}");
                vnpay.AddRequestData("vnp_OrderType", "other");
                vnpay.AddRequestData("vnp_ReturnUrl", request.ReturnUrl);
                vnpay.AddRequestData("vnp_TxnRef", $"GYM{payment.ThanhToanId}{DateTime.Now:yyyyMMddHHmmss}");

                // Create payment gateway record
                var orderId = $"GYM{payment.ThanhToanId}{DateTime.Now:yyyyMMddHHmmss}";
                var gateway = new ThanhToanGateway
                {
                    ThanhToanId = payment.ThanhToanId,
                    GatewayTen = "VNPAY",
                    GatewayOrderId = orderId,
                    GatewayAmount = payment.SoTien,
                    ThoiGianCallback = DateTime.Now
                };

                _context.ThanhToanGateways.Add(gateway);
                await _context.SaveChangesAsync();

                var paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

                _logger.LogInformation($"VNPay payment URL created for ThanhToanId: {payment.ThanhToanId}");
                
                return Json(new { success = true, paymentUrl = paymentUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating VNPay payment URL");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo thanh toán" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentConfirm()
        {
            try
            {
                var vnpayData = new Dictionary<string, string>();
                foreach (var key in Request.Query.Keys)
                {
                    vnpayData.Add(key, Request.Query[key]);
                }

                var vnpay = new VnPayLibrary();
                foreach (var item in vnpayData)
                {
                    vnpay.AddResponseData(item.Key, item.Value);
                }

                var vnp_OrderId = vnpay.GetResponseData("vnp_TxnRef");
                var vnp_TransactionId = vnpay.GetResponseData("vnp_TransactionNo");
                var vnp_SecureHash = vnpayData["vnp_SecureHash"];
                var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");

                var vnp_HashSecret = _configuration.GetSection("VnPay")["HashSecret"];
                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);

                if (!checkSignature)
                {
                    _logger.LogError("VNPay signature validation failed");
                    return Redirect("/Member/MyRegistrations?paymentStatus=error&message=Chữ+ký+không+hợp+lệ");
                }

                // Find gateway record
                var gateway = await _context.ThanhToanGateways
                    .Include(g => g.ThanhToan)
                    .FirstOrDefaultAsync(g => g.GatewayOrderId == vnp_OrderId);

                if (gateway == null)
                {
                    _logger.LogError($"Gateway not found for order ID: {vnp_OrderId}");
                    return Redirect("/Member/MyRegistrations?paymentStatus=error&message=Không+tìm+thấy+giao+dịch");
                }

                // Update gateway with response data
                gateway.GatewayTransId = vnp_TransactionId;
                gateway.GatewayRespCode = vnp_ResponseCode;
                gateway.ThoiGianCallback = DateTime.Now;

                if (vnp_ResponseCode == "00") // Success
                {
                    gateway.ThanhToan.TrangThai = "SUCCESS";
                    gateway.GatewayMessage = "Thanh toán thành công";

                    // Activate pending registration if exists
                    if (gateway.ThanhToan.DangKyId.HasValue)
                    {
                        var registration = await _context.DangKys
                            .FirstOrDefaultAsync(d => d.DangKyId == gateway.ThanhToan.DangKyId.Value);
                        
                        if (registration != null && registration.TrangThai == "PENDING_PAYMENT")
                        {
                            registration.TrangThai = "ACTIVE";
                            registration.TrangThaiChiTiet = "Thanh toán thành công";
                        }
                    }

                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"VNPay payment successful for order: {vnp_OrderId}");
                    return Redirect("/Member/MyRegistrations?paymentStatus=success&message=Thanh+toán+thành+công");
                }
                else
                {
                    gateway.ThanhToan.TrangThai = "FAILED";
                    gateway.GatewayMessage = "Thanh toán thất bại";

                    // Cancel pending registration if exists
                    if (gateway.ThanhToan.DangKyId.HasValue)
                    {
                        var registration = await _context.DangKys
                            .FirstOrDefaultAsync(d => d.DangKyId == gateway.ThanhToan.DangKyId.Value);
                        
                        if (registration != null && registration.TrangThai == "PENDING_PAYMENT")
                        {
                            registration.TrangThai = "CANCELLED";
                            registration.LyDoHuy = "Thanh toán thất bại";
                        }
                    }

                    await _context.SaveChangesAsync();
                    
                    _logger.LogWarning($"VNPay payment failed for order: {vnp_OrderId}, response code: {vnp_ResponseCode}");
                    return Redirect("/Member/MyRegistrations?paymentStatus=error&message=Thanh+toán+thất+bại");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay callback");
                return Redirect("/Member/MyRegistrations?paymentStatus=error&message=Có+lỗi+xảy+ra+khi+xử+lý+thanh+toán");
            }
        }
    }

    public class PaymentRequest
    {
        public int ThanhToanId { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;
    }

    public static class Utils
    {
        public static string GetIpAddress(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress;
            if (ipAddress != null)
            {
                if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    ipAddress = System.Net.Dns.GetHostEntry(ipAddress).AddressList
                        .FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                }
            }
            return ipAddress?.ToString() ?? "127.0.0.1";
        }
    }
} 