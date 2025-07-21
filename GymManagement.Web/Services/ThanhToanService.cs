using GymManagement.Web.Data;
using GymManagement.Web.Data.Models;
using GymManagement.Web.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace GymManagement.Web.Services
{
    public class ThanhToanService : IThanhToanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IThanhToanRepository _thanhToanRepository;
        private readonly IDangKyRepository _dangKyRepository;
        private readonly IThongBaoService _thongBaoService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public ThanhToanService(
            IUnitOfWork unitOfWork,
            IThanhToanRepository thanhToanRepository,
            IDangKyRepository dangKyRepository,
            IThongBaoService thongBaoService,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _thanhToanRepository = thanhToanRepository;
            _dangKyRepository = dangKyRepository;
            _thongBaoService = thongBaoService;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<IEnumerable<ThanhToan>> GetAllAsync()
        {
            return await _thanhToanRepository.GetAllAsync();
        }

        public async Task<ThanhToan?> GetByIdAsync(int id)
        {
            return await _thanhToanRepository.GetByIdAsync(id);
        }

        public async Task<ThanhToan> CreateAsync(ThanhToan thanhToan)
        {
            var created = await _thanhToanRepository.AddAsync(thanhToan);
            await _unitOfWork.SaveChangesAsync();
            return created;
        }

        public async Task<ThanhToan> UpdateAsync(ThanhToan thanhToan)
        {
            await _thanhToanRepository.UpdateAsync(thanhToan);
            await _unitOfWork.SaveChangesAsync();
            return thanhToan;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var thanhToan = await _thanhToanRepository.GetByIdAsync(id);
            if (thanhToan == null) return false;

            await _thanhToanRepository.DeleteAsync(thanhToan);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ThanhToan>> GetByRegistrationIdAsync(int dangKyId)
        {
            return await _thanhToanRepository.GetByDangKyIdAsync(dangKyId);
        }

        public async Task<IEnumerable<ThanhToan>> GetPendingPaymentsAsync()
        {
            return await _thanhToanRepository.GetPendingPaymentsAsync();
        }

        public async Task<IEnumerable<ThanhToan>> GetSuccessfulPaymentsAsync()
        {
            return await _thanhToanRepository.GetSuccessfulPaymentsAsync();
        }

        public async Task<ThanhToan> CreatePaymentAsync(int dangKyId, decimal soTien, string phuongThuc, string? ghiChu = null)
        {
            var thanhToan = new ThanhToan
            {
                DangKyId = dangKyId,
                SoTien = soTien,
                PhuongThuc = phuongThuc,
                TrangThai = "PENDING",
                NgayThanhToan = DateTime.Now,
                GhiChu = ghiChu
            };

            var created = await _thanhToanRepository.AddAsync(thanhToan);
            await _unitOfWork.SaveChangesAsync();
            return created;
        }

        public async Task<bool> ProcessCashPaymentAsync(int thanhToanId)
        {
            var thanhToan = await _thanhToanRepository.GetPaymentWithGatewayAsync(thanhToanId);
            if (thanhToan == null || thanhToan.TrangThai != "PENDING") return false;

            thanhToan.TrangThai = "SUCCESS";
            thanhToan.NgayThanhToan = DateTime.Now;
            await _unitOfWork.SaveChangesAsync();

            // Send notifications
            await SendPaymentSuccessNotifications(thanhToan);

            return true;
        }

        public async Task<string> CreateVnPayUrlAsync(int thanhToanId, string returnUrl)
        {
            var thanhToan = await _thanhToanRepository.GetPaymentWithGatewayAsync(thanhToanId);
            if (thanhToan == null) throw new ArgumentException("Payment not found");

            var vnpayConfig = _configuration.GetSection("VnPay");
            var tmnCode = vnpayConfig["TmnCode"];
            var hashSecret = vnpayConfig["HashSecret"];
            var baseUrl = vnpayConfig["BaseUrl"];

            var orderId = $"GYM{thanhToanId}{DateTime.Now:yyyyMMddHHmmss}";
            var amount = (long)(thanhToan.SoTien * 100); // VNPay requires amount in VND cents

            var vnpayData = new SortedDictionary<string, string>
            {
                {"vnp_Version", "2.1.0"},
                {"vnp_Command", "pay"},
                {"vnp_TmnCode", tmnCode},
                {"vnp_Amount", amount.ToString()},
                {"vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")},
                {"vnp_CurrCode", "VND"},
                {"vnp_IpAddr", "127.0.0.1"},
                {"vnp_Locale", "vn"},
                {"vnp_OrderInfo", $"Thanh toan gym {orderId}"},
                {"vnp_OrderType", "other"},
                {"vnp_ReturnUrl", returnUrl},
                {"vnp_TxnRef", orderId}
            };

            // Create gateway record
            var gateway = new ThanhToanGateway
            {
                ThanhToanId = thanhToanId,
                GatewayTen = "VNPAY",
                GatewayOrderId = orderId,
                GatewayAmount = thanhToan.SoTien
            };

            await _unitOfWork.Context.ThanhToanGateways.AddAsync(gateway);
            await _unitOfWork.SaveChangesAsync();

            // Create signature
            var signData = string.Join("&", vnpayData.Select(kv => $"{kv.Key}={kv.Value}"));
            var signature = CreateHmacSha512(hashSecret, signData);
            vnpayData.Add("vnp_SecureHash", signature);

            // Build URL
            var queryString = string.Join("&", vnpayData.Select(kv => $"{kv.Key}={HttpUtility.UrlEncode(kv.Value)}"));
            return $"{baseUrl}?{queryString}";
        }

        public async Task<bool> ProcessVnPayReturnAsync(Dictionary<string, string> vnpayData)
        {
            var vnpayConfig = _configuration.GetSection("VnPay");
            var hashSecret = vnpayConfig["HashSecret"];

            // Verify signature
            var secureHash = vnpayData["vnp_SecureHash"];
            vnpayData.Remove("vnp_SecureHash");

            var sortedData = new SortedDictionary<string, string>(vnpayData);
            var signData = string.Join("&", sortedData.Select(kv => $"{kv.Key}={kv.Value}"));
            var computedHash = CreateHmacSha512(hashSecret, signData);

            if (secureHash != computedHash) return false;

            // Process payment result
            var orderId = vnpayData["vnp_TxnRef"];
            var responseCode = vnpayData["vnp_ResponseCode"];
            var transactionId = vnpayData["vnp_TransactionNo"];

            var gateway = await _unitOfWork.Context.ThanhToanGateways
                .Include(g => g.ThanhToan)
                .FirstOrDefaultAsync(g => g.GatewayOrderId == orderId);

            if (gateway == null) return false;

            gateway.GatewayTransId = transactionId;
            gateway.GatewayRespCode = responseCode;
            gateway.ThoiGianCallback = DateTime.Now;

            if (responseCode == "00") // Success
            {
                gateway.ThanhToan.TrangThai = "SUCCESS";
                gateway.GatewayMessage = "Thanh toán thành công";

                // Send notifications
                await SendPaymentSuccessNotifications(gateway.ThanhToan);
            }
            else
            {
                gateway.ThanhToan.TrangThai = "FAILED";
                gateway.GatewayMessage = "Thanh toán thất bại";
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RefundPaymentAsync(int thanhToanId, string reason)
        {
            var thanhToan = await _thanhToanRepository.GetByIdAsync(thanhToanId);
            if (thanhToan == null || thanhToan.TrangThai != "SUCCESS") return false;

            thanhToan.TrangThai = "REFUND";
            thanhToan.GhiChu = $"Hoàn tiền: {reason}";
            await _unitOfWork.SaveChangesAsync();

            // Send notification
            var dangKy = await _dangKyRepository.GetByIdAsync(thanhToan.DangKyId);
            if (dangKy != null)
            {
                await _thongBaoService.CreateNotificationAsync(
                    dangKy.NguoiDungId,
                    "Hoàn tiền",
                    $"Đã hoàn tiền {thanhToan.SoTien:N0} VNĐ. Lý do: {reason}",
                    "APP"
                );
            }

            return true;
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            return await _thanhToanRepository.GetTotalRevenueByDateRangeAsync(startDate, endDate);
        }

        private async Task SendPaymentSuccessNotifications(ThanhToan thanhToan)
        {
            var dangKy = await _dangKyRepository.GetByIdAsync(thanhToan.DangKyId);
            if (dangKy?.NguoiDung != null)
            {
                // Send in-app notification
                await _thongBaoService.CreateNotificationAsync(
                    dangKy.NguoiDungId,
                    "Thanh toán thành công",
                    $"Thanh toán {thanhToan.SoTien:N0} VNĐ đã được xử lý thành công",
                    "APP"
                );

                // Send email confirmation
                if (!string.IsNullOrEmpty(dangKy.NguoiDung.Email))
                {
                    var memberName = $"{dangKy.NguoiDung.Ho} {dangKy.NguoiDung.Ten}".Trim();
                    await _emailService.SendPaymentConfirmationEmailAsync(
                        dangKy.NguoiDung.Email,
                        memberName,
                        thanhToan.SoTien,
                        thanhToan.PhuongThuc ?? "Unknown"
                    );
                }
            }
        }

        private string CreateHmacSha512(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(dataBytes);
            return Convert.ToHexString(hashBytes).ToLower();
        }
    }
}
