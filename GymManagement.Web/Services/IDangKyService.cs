using GymManagement.Web.Data.Models;

namespace GymManagement.Web.Services
{
    public interface IDangKyService
    {
        Task<IEnumerable<DangKy>> GetAllAsync();
        Task<DangKy?> GetByIdAsync(int id);
        Task<DangKy> CreateAsync(DangKy dangKy);
        Task<DangKy> UpdateAsync(DangKy dangKy);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<DangKy>> GetByMemberIdAsync(int nguoiDungId);
        Task<IEnumerable<DangKy>> GetActiveRegistrationsAsync();
        Task<IEnumerable<DangKy>> GetExpiredRegistrationsAsync();
        Task<bool> RegisterPackageAsync(int nguoiDungId, int goiTapId, int thoiHanThang, int? khuyenMaiId = null);
        Task<bool> RegisterClassAsync(int nguoiDungId, int lopHocId, DateTime ngayBatDau, DateTime ngayKetThuc);
        Task<bool> ExtendRegistrationAsync(int dangKyId, int additionalMonths);
        Task<bool> CancelRegistrationAsync(int dangKyId, string reason);
        Task<decimal> CalculateRegistrationFeeAsync(int goiTapId, int thoiHanThang, int? khuyenMaiId = null);
        Task ProcessExpiredRegistrationsAsync();
    }
}
