using GymManagement.Web.Data;
using GymManagement.Web.Data.Models;
using GymManagement.Web.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Web.Services
{
    public class DangKyService : IDangKyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDangKyRepository _dangKyRepository;
        private readonly IGoiTapRepository _goiTapRepository;
        private readonly ILopHocRepository _lopHocRepository;
        private readonly IThongBaoService _thongBaoService;

        public DangKyService(
            IUnitOfWork unitOfWork,
            IDangKyRepository dangKyRepository,
            IGoiTapRepository goiTapRepository,
            ILopHocRepository lopHocRepository,
            IThongBaoService thongBaoService)
        {
            _unitOfWork = unitOfWork;
            _dangKyRepository = dangKyRepository;
            _goiTapRepository = goiTapRepository;
            _lopHocRepository = lopHocRepository;
            _thongBaoService = thongBaoService;
        }

        public async Task<IEnumerable<DangKy>> GetAllAsync()
        {
            return await _dangKyRepository.GetAllAsync();
        }

        public async Task<DangKy?> GetByIdAsync(int id)
        {
            return await _dangKyRepository.GetByIdAsync(id);
        }

        public async Task<DangKy> CreateAsync(DangKy dangKy)
        {
            var created = await _dangKyRepository.AddAsync(dangKy);
            await _unitOfWork.SaveChangesAsync();
            return created;
        }

        public async Task<DangKy> UpdateAsync(DangKy dangKy)
        {
            await _dangKyRepository.UpdateAsync(dangKy);
            await _unitOfWork.SaveChangesAsync();
            return dangKy;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var dangKy = await _dangKyRepository.GetByIdAsync(id);
            if (dangKy == null) return false;

            await _dangKyRepository.DeleteAsync(dangKy);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<DangKy>> GetByMemberIdAsync(int nguoiDungId)
        {
            return await _dangKyRepository.GetByNguoiDungIdAsync(nguoiDungId);
        }

        public async Task<IEnumerable<DangKy>> GetActiveRegistrationsAsync()
        {
            return await _dangKyRepository.GetActiveRegistrationsAsync();
        }

        public async Task<IEnumerable<DangKy>> GetExpiredRegistrationsAsync()
        {
            return await _dangKyRepository.GetExpiredRegistrationsAsync();
        }

        public async Task<bool> RegisterPackageAsync(int nguoiDungId, int goiTapId, int thoiHanThang, int? khuyenMaiId = null)
        {
            // Check if package exists
            var goiTap = await _goiTapRepository.GetByIdAsync(goiTapId);
            if (goiTap == null) return false;

            // Check if user already has active registration for this package
            if (await _dangKyRepository.HasActiveRegistrationAsync(nguoiDungId, goiTapId, null))
                return false;

            // Calculate dates
            var ngayBatDau = DateOnly.FromDateTime(DateTime.Today);
            var ngayKetThuc = DateOnly.FromDateTime(DateTime.Today.AddMonths(thoiHanThang));

            // Create registration
            var dangKy = new DangKy
            {
                NguoiDungId = nguoiDungId,
                GoiTapId = goiTapId,
                NgayBatDau = ngayBatDau,
                NgayKetThuc = ngayKetThuc,
                TrangThai = "ACTIVE",
                NgayTao = DateTime.Now
            };

            await _dangKyRepository.AddAsync(dangKy);
            await _unitOfWork.SaveChangesAsync();

            // Send notification
            await _thongBaoService.CreateNotificationAsync(
                nguoiDungId,
                "Đăng ký gói tập thành công",
                $"Bạn đã đăng ký thành công gói {goiTap.TenGoi}. Thời hạn: {ngayBatDau:dd/MM/yyyy} - {ngayKetThuc:dd/MM/yyyy}",
                "APP"
            );

            return true;
        }

        public async Task<bool> RegisterClassAsync(int nguoiDungId, int lopHocId, DateTime ngayBatDau, DateTime ngayKetThuc)
        {
            // Check if class exists
            var lopHoc = await _lopHocRepository.GetByIdAsync(lopHocId);
            if (lopHoc == null || lopHoc.TrangThai != "OPEN") return false;

            // Check if user already has active registration for this class
            if (await _dangKyRepository.HasActiveRegistrationAsync(nguoiDungId, null, lopHocId))
                return false;

            // Create registration
            var dangKy = new DangKy
            {
                NguoiDungId = nguoiDungId,
                LopHocId = lopHocId,
                NgayBatDau = DateOnly.FromDateTime(ngayBatDau),
                NgayKetThuc = DateOnly.FromDateTime(ngayKetThuc),
                TrangThai = "ACTIVE",
                NgayTao = DateTime.Now
            };

            await _dangKyRepository.AddAsync(dangKy);
            await _unitOfWork.SaveChangesAsync();

            // Send notification
            await _thongBaoService.CreateNotificationAsync(
                nguoiDungId,
                "Đăng ký lớp học thành công",
                $"Bạn đã đăng ký thành công lớp {lopHoc.TenLop}. Thời hạn: {ngayBatDau:dd/MM/yyyy} - {ngayKetThuc:dd/MM/yyyy}",
                "APP"
            );

            return true;
        }

        public async Task<bool> ExtendRegistrationAsync(int dangKyId, int additionalMonths)
        {
            var dangKy = await _dangKyRepository.GetByIdAsync(dangKyId);
            if (dangKy == null || dangKy.TrangThai != "ACTIVE") return false;

            dangKy.NgayKetThuc = dangKy.NgayKetThuc.AddMonths(additionalMonths);
            await _unitOfWork.SaveChangesAsync();

            // Send notification
            await _thongBaoService.CreateNotificationAsync(
                dangKy.NguoiDungId,
                "Gia hạn đăng ký thành công",
                $"Đăng ký của bạn đã được gia hạn thêm {additionalMonths} tháng. Ngày hết hạn mới: {dangKy.NgayKetThuc:dd/MM/yyyy}",
                "APP"
            );

            return true;
        }

        public async Task<bool> CancelRegistrationAsync(int dangKyId, string reason)
        {
            var dangKy = await _dangKyRepository.GetByIdAsync(dangKyId);
            if (dangKy == null) return false;

            dangKy.TrangThai = "CANCELED";
            await _unitOfWork.SaveChangesAsync();

            // Send notification
            await _thongBaoService.CreateNotificationAsync(
                dangKy.NguoiDungId,
                "Huỷ đăng ký",
                $"Đăng ký của bạn đã bị huỷ. Lý do: {reason}",
                "APP"
            );

            return true;
        }

        public async Task<decimal> CalculateRegistrationFeeAsync(int goiTapId, int thoiHanThang, int? khuyenMaiId = null)
        {
            var goiTap = await _goiTapRepository.GetByIdAsync(goiTapId);
            if (goiTap == null) return 0;

            decimal totalFee = goiTap.Gia * thoiHanThang;

            // Apply discount if available
            if (khuyenMaiId.HasValue)
            {
                var khuyenMai = await _unitOfWork.Context.KhuyenMais.FindAsync(khuyenMaiId.Value);
                if (khuyenMai != null && khuyenMai.KichHoat &&
                    DateOnly.FromDateTime(DateTime.Today) >= khuyenMai.NgayBatDau && DateOnly.FromDateTime(DateTime.Today) <= khuyenMai.NgayKetThuc)
                {
                    decimal discount = totalFee * (khuyenMai.PhanTramGiam ?? 0) / 100;
                    totalFee -= discount;
                }
            }

            return totalFee;
        }

        public async Task ProcessExpiredRegistrationsAsync()
        {
            var expiredRegistrations = await _dangKyRepository.GetExpiredRegistrationsAsync();
            
            foreach (var dangKy in expiredRegistrations)
            {
                dangKy.TrangThai = "EXPIRED";
                
                // Send notification
                await _thongBaoService.CreateNotificationAsync(
                    dangKy.NguoiDungId,
                    "Đăng ký đã hết hạn",
                    "Đăng ký của bạn đã hết hạn. Vui lòng gia hạn để tiếp tục sử dụng dịch vụ.",
                    "APP"
                );
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
