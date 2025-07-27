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

            // 🔒 CHÍNH SÁCH: Mỗi thành viên chỉ có thể sở hữu một gói tập tại một thời điểm
            // Check if user already has ANY active package registration
            var existingActivePackages = await _dangKyRepository.GetByMemberIdAsync(nguoiDungId);
            var hasActivePackage = existingActivePackages.Any(d =>
                d.GoiTapId != null &&
                d.TrangThai == "ACTIVE" &&
                d.NgayKetThuc >= DateOnly.FromDateTime(DateTime.Today));

            if (hasActivePackage)
            {
                // User already has an active package, cannot register another one
                return false;
            }

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

            // Check if user already has active registration for this specific class
            if (await _dangKyRepository.HasActiveRegistrationAsync(nguoiDungId, null, lopHocId))
                return false;

            // 🧘‍♂️ CHÍNH SÁCH: Thành viên có thể đăng ký nhiều lớp học cùng lúc
            // Kiểm tra trùng lịch thời gian với các lớp đã đăng ký
            var existingActiveClasses = await _dangKyRepository.GetByMemberIdAsync(nguoiDungId);
            var activeClassRegistrations = existingActiveClasses.Where(d =>
                d.LopHocId != null &&
                d.TrangThai == "ACTIVE" &&
                d.NgayKetThuc >= DateOnly.FromDateTime(DateTime.Today)).ToList();

            // Check for time conflicts
            foreach (var existingReg in activeClassRegistrations)
            {
                if (existingReg.LopHoc != null && HasTimeConflict(lopHoc, existingReg.LopHoc))
                {
                    // Time conflict detected
                    return false;
                }
            }

            // Check class capacity
            var currentRegistrations = await _dangKyRepository.GetActiveRegistrationsAsync();
            var classRegistrationCount = currentRegistrations.Count(d => d.LopHocId == lopHocId);
            if (classRegistrationCount >= lopHoc.SucChua)
            {
                // Class is full
                return false;
            }

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

        public async Task<IEnumerable<DangKy>> GetActiveMemberRegistrationsAsync(int nguoiDungId)
        {
            var allRegistrations = await _dangKyRepository.GetByMemberIdAsync(nguoiDungId);
            return allRegistrations.Where(d => d.TrangThai == "ACTIVE" && d.NgayKetThuc >= DateOnly.FromDateTime(DateTime.Today));
        }

        public async Task<IEnumerable<DangKy>> GetActiveRegistrationsByMemberIdAsync(int nguoiDungId)
        {
            var allRegistrations = await _dangKyRepository.GetByMemberIdAsync(nguoiDungId);
            return allRegistrations.Where(d => d.TrangThai == "ACTIVE" && d.NgayKetThuc >= DateOnly.FromDateTime(DateTime.Today));
        }

        /// <summary>
        /// Kiểm tra xung đột thời gian giữa hai lớp học
        /// </summary>
        private bool HasTimeConflict(LopHoc newClass, LopHoc existingClass)
        {
            // Parse days of week for both classes
            var newClassDays = ParseDaysOfWeek(newClass.ThuTrongTuan);
            var existingClassDays = ParseDaysOfWeek(existingClass.ThuTrongTuan);

            // Check if there's any common day
            var commonDays = newClassDays.Intersect(existingClassDays);
            if (!commonDays.Any())
            {
                // No common days, no conflict
                return false;
            }

            // Check time overlap on common days
            var newStart = newClass.GioBatDau;
            var newEnd = newClass.GioKetThuc;
            var existingStart = existingClass.GioBatDau;
            var existingEnd = existingClass.GioKetThuc;

            // Time conflict if:
            // New class starts before existing ends AND new class ends after existing starts
            return newStart < existingEnd && newEnd > existingStart;
        }

        /// <summary>
        /// Parse days of week string to list of integers (1=Monday, 7=Sunday)
        /// </summary>
        private List<int> ParseDaysOfWeek(string thuTrongTuan)
        {
            var days = new List<int>();
            if (string.IsNullOrEmpty(thuTrongTuan)) return days;

            // Handle different formats: "2,4,6" or "Thứ 2, Thứ 4, Thứ 6"
            var dayStrings = thuTrongTuan.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var dayStr in dayStrings)
            {
                var cleanDay = dayStr.Trim().ToLower();

                // Try to parse as number first
                if (int.TryParse(cleanDay, out int dayNum) && dayNum >= 1 && dayNum <= 7)
                {
                    days.Add(dayNum);
                }
                // Parse Vietnamese day names
                else if (cleanDay.Contains("2") || cleanDay.Contains("hai"))
                    days.Add(2);
                else if (cleanDay.Contains("3") || cleanDay.Contains("ba"))
                    days.Add(3);
                else if (cleanDay.Contains("4") || cleanDay.Contains("tư"))
                    days.Add(4);
                else if (cleanDay.Contains("5") || cleanDay.Contains("năm"))
                    days.Add(5);
                else if (cleanDay.Contains("6") || cleanDay.Contains("sáu"))
                    days.Add(6);
                else if (cleanDay.Contains("7") || cleanDay.Contains("bảy"))
                    days.Add(7);
                else if (cleanDay.Contains("chủ nhật") || cleanDay.Contains("cn"))
                    days.Add(1);
            }

            return days.Distinct().ToList();
        }

        /// <summary>
        /// Kiểm tra xem member có gói tập đang hoạt động không
        /// </summary>
        public async Task<bool> HasActivePackageAsync(int nguoiDungId)
        {
            var registrations = await _dangKyRepository.GetByMemberIdAsync(nguoiDungId);
            return registrations.Any(d =>
                d.GoiTapId != null &&
                d.TrangThai == "ACTIVE" &&
                d.NgayKetThuc >= DateOnly.FromDateTime(DateTime.Today));
        }

        /// <summary>
        /// Kiểm tra xem member có thể đăng ký lớp học không (không trùng lịch và lớp chưa đầy)
        /// </summary>
        public async Task<bool> CanRegisterClassAsync(int nguoiDungId, int lopHocId)
        {
            // Check if class exists and is open
            var lopHoc = await _lopHocRepository.GetByIdAsync(lopHocId);
            if (lopHoc == null || lopHoc.TrangThai != "OPEN") return false;

            // Check if user already registered for this class
            if (await _dangKyRepository.HasActiveRegistrationAsync(nguoiDungId, null, lopHocId))
                return false;

            // Check class capacity
            var currentCount = await GetActiveClassRegistrationCountAsync(lopHocId);
            if (currentCount >= lopHoc.SucChua) return false;

            // Check time conflicts
            var existingActiveClasses = await _dangKyRepository.GetByMemberIdAsync(nguoiDungId);
            var activeClassRegistrations = existingActiveClasses.Where(d =>
                d.LopHocId != null &&
                d.TrangThai == "ACTIVE" &&
                d.NgayKetThuc >= DateOnly.FromDateTime(DateTime.Today)).ToList();

            foreach (var existingReg in activeClassRegistrations)
            {
                if (existingReg.LopHoc != null && HasTimeConflict(lopHoc, existingReg.LopHoc))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Đếm số lượng đăng ký đang hoạt động cho một lớp học
        /// </summary>
        public async Task<int> GetActiveClassRegistrationCountAsync(int lopHocId)
        {
            var activeRegistrations = await _dangKyRepository.GetActiveRegistrationsAsync();
            return activeRegistrations.Count(d => d.LopHocId == lopHocId);
        }
    }
}
