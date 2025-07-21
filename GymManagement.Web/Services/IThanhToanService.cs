using GymManagement.Web.Data.Models;

namespace GymManagement.Web.Services
{
    public interface IThanhToanService
    {
        Task<IEnumerable<ThanhToan>> GetAllAsync();
        Task<ThanhToan?> GetByIdAsync(int id);
        Task<ThanhToan> CreateAsync(ThanhToan thanhToan);
        Task<ThanhToan> UpdateAsync(ThanhToan thanhToan);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ThanhToan>> GetByRegistrationIdAsync(int dangKyId);
        Task<IEnumerable<ThanhToan>> GetPendingPaymentsAsync();
        Task<IEnumerable<ThanhToan>> GetSuccessfulPaymentsAsync();
        Task<ThanhToan> CreatePaymentAsync(int dangKyId, decimal soTien, string phuongThuc, string? ghiChu = null);
        Task<bool> ProcessCashPaymentAsync(int thanhToanId);
        Task<string> CreateVnPayUrlAsync(int thanhToanId, string returnUrl);
        Task<bool> ProcessVnPayReturnAsync(Dictionary<string, string> vnpayData);
        Task<bool> RefundPaymentAsync(int thanhToanId, string reason);
        Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
    }
}
