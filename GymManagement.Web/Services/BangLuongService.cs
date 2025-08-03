using GymManagement.Web.Data;
using GymManagement.Web.Data.Models;
using GymManagement.Web.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Web.Services
{
    public class BangLuongService : IBangLuongService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBangLuongRepository _bangLuongRepository;
        private readonly INguoiDungRepository _nguoiDungRepository;
        private readonly IThongBaoService _thongBaoService;
        private readonly IEmailService _emailService;

        public BangLuongService(
            IUnitOfWork unitOfWork,
            IBangLuongRepository bangLuongRepository,
            INguoiDungRepository nguoiDungRepository,
            IThongBaoService thongBaoService,
            IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _bangLuongRepository = bangLuongRepository;
            _nguoiDungRepository = nguoiDungRepository;
            _thongBaoService = thongBaoService;
            _emailService = emailService;
        }

        public async Task<IEnumerable<BangLuong>> GetAllAsync()
        {
            return await _bangLuongRepository.GetAllAsync();
        }

        public async Task<BangLuong?> GetByIdAsync(int id)
        {
            return await _bangLuongRepository.GetByIdAsync(id);
        }

        public async Task<BangLuong> CreateAsync(BangLuong bangLuong)
        {
            var created = await _bangLuongRepository.AddAsync(bangLuong);
            await _unitOfWork.SaveChangesAsync();
            return created;
        }

        public async Task<BangLuong> UpdateAsync(BangLuong bangLuong)
        {
            await _bangLuongRepository.UpdateAsync(bangLuong);
            await _unitOfWork.SaveChangesAsync();
            return bangLuong;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var bangLuong = await _bangLuongRepository.GetByIdAsync(id);
            if (bangLuong == null) return false;

            await _bangLuongRepository.DeleteAsync(bangLuong);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BangLuong>> GetByTrainerIdAsync(int hlvId)
        {
            return await _bangLuongRepository.GetByHlvIdAsync(hlvId);
        }

        public async Task<IEnumerable<BangLuong>> GetByMonthAsync(string thang)
        {
            return await _bangLuongRepository.GetByMonthAsync(thang);
        }

        public async Task<BangLuong?> GetByTrainerAndMonthAsync(int hlvId, string thang)
        {
            return await _bangLuongRepository.GetByHlvAndMonthAsync(hlvId, thang);
        }

        public async Task<IEnumerable<BangLuong>> GetUnpaidSalariesAsync()
        {
            return await _bangLuongRepository.GetUnpaidSalariesAsync();
        }

        public async Task<bool> GenerateMonthlySalariesAsync(string thang)
        {
            // Get all active trainers
            var trainers = await _nguoiDungRepository.GetTrainersAsync();
            
            foreach (var trainer in trainers)
            {
                // Check if salary already exists for this month
                var existingSalary = await _bangLuongRepository.GetByHlvAndMonthAsync(trainer.NguoiDungId, thang);
                if (existingSalary != null) continue;

                // Calculate base salary (this could be configurable)
                decimal baseSalary = 10000000; // 10 million VND base salary

                // Calculate detailed commission breakdown
                var commissionBreakdown = await CalculateDetailedCommissionAsync(trainer.NguoiDungId, thang);

                // Create salary record - simplified
                var bangLuong = new BangLuong
                {
                    HlvId = trainer.NguoiDungId,
                    Thang = thang,
                    LuongCoBan = baseSalary,
                    TienHoaHong = commissionBreakdown.TotalCommission
                };

                await _bangLuongRepository.AddAsync(bangLuong);

                // Send notification
                await _thongBaoService.CreateNotificationAsync(
                    trainer.NguoiDungId,
                    "Bảng lương tháng mới",
                    $"Bảng lương tháng {thang} đã được tạo.\n" +
                    $"Lương cơ bản: {baseSalary:N0} VNĐ\n" +
                    $"Hoa hồng: {commissionBreakdown.TotalCommission:N0} VNĐ\n" +
                    $"Tổng cộng: {bangLuong.TongThanhToan:N0} VNĐ",
                    "APP"
                );

                // Send detailed email if available
                if (!string.IsNullOrEmpty(trainer.Email))
                {
                    await SendDetailedSalaryEmailAsync(trainer, bangLuong, commissionBreakdown, thang);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private async Task SendDetailedSalaryEmailAsync(NguoiDung trainer, BangLuong bangLuong, CommissionBreakdown breakdown, string thang)
        {
            var subject = $"Bảng lương chi tiết tháng {thang}";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .breakdown {{ background-color: #f8f9fa; padding: 15px; margin: 10px 0; border-radius: 5px; }}
                        .metrics {{ background-color: #e9ecef; padding: 15px; margin: 10px 0; border-radius: 5px; }}
                        .total {{ background-color: #d4edda; padding: 15px; margin: 10px 0; border-radius: 5px; font-weight: bold; }}
                        table {{ width: 100%; border-collapse: collapse; margin: 10px 0; }}
                        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                        th {{ background-color: #f2f2f2; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h2>Bảng Lương Chi Tiết Tháng {thang}</h2>
                        <p>Huấn luyện viên: {trainer.Ho} {trainer.Ten}</p>
                    </div>
                    
                    <div class='content'>
                        <div class='breakdown'>
                            <h3>Chi tiết lương và hoa hồng</h3>
                            <table>
                                <tr><th>Khoản mục</th><th>Số tiền (VNĐ)</th></tr>
                                <tr><td>Lương cơ bản</td><td>{bangLuong.LuongCoBan:N0}</td></tr>
                                <tr><td>Hoa hồng gói tập</td><td>{breakdown.PackageCommission:N0}</td></tr>
                                <tr><td>Hoa hồng lớp học</td><td>{breakdown.ClassCommission:N0}</td></tr>
                                <tr><td>Hoa hồng cá nhân</td><td>{breakdown.PersonalCommission:N0}</td></tr>
                                <tr><td>Thưởng hiệu suất</td><td>{breakdown.PerformanceBonus:N0}</td></tr>
                                <tr><td>Thưởng điểm danh</td><td>{breakdown.AttendanceBonus:N0}</td></tr>
                            </table>
                        </div>

                        <div class='metrics'>
                            <h3>Hiệu suất tháng {thang}</h3>
                            <table>
                                <tr><th>Chỉ số</th><th>Giá trị</th></tr>
                                <tr><td>Số lượng học viên</td><td>{breakdown.StudentCount} học viên</td></tr>
                                <tr><td>Số buổi dạy</td><td>{breakdown.ClassesTaught} buổi</td></tr>
                                <tr><td>Số buổi cá nhân</td><td>{breakdown.PersonalSessions} buổi</td></tr>
                                <tr><td>Tỷ lệ điểm danh</td><td>{breakdown.AttendanceRate:F1}%</td></tr>
                            </table>
                        </div>

                        <div class='total'>
                            <h3>Tổng thanh toán: {bangLuong.TongThanhToan:N0} VNĐ</h3>
                        </div>

                        <p><strong>Ghi chú:</strong></p>
                        <ul>
                            <li>Bảng lương được tính dựa trên hiệu suất và doanh thu thực tế</li>
                            <li>Hoa hồng được tính từ các gói tập và lớp học đã thanh toán thành công</li>
                            <li>Thưởng hiệu suất dựa trên số lượng học viên và tỷ lệ điểm danh</li>
                            <li>Vui lòng liên hệ bộ phận nhân sự nếu có thắc mắc</li>
                        </ul>

                        <p>Trân trọng,<br/>Hệ thống quản lý phòng Gym</p>
                    </div>
                </body>
                </html>";

            await _emailService.SendEmailAsync(trainer.Email, $"{trainer.Ho} {trainer.Ten}", subject, body);
        }

        public async Task<bool> PaySalaryAsync(int bangLuongId)
        {
            var bangLuong = await _bangLuongRepository.GetByIdAsync(bangLuongId);
            if (bangLuong == null || bangLuong.NgayThanhToan != null) return false;

            bangLuong.NgayThanhToan = DateOnly.FromDateTime(DateTime.Today);
            await _unitOfWork.SaveChangesAsync();

            // Send notifications
            var trainer = await _nguoiDungRepository.GetByIdAsync(bangLuong.HlvId ?? 0);
            if (trainer != null)
            {
                await _thongBaoService.CreateNotificationAsync(
                    bangLuong.HlvId ?? 0,
                    "Đã thanh toán lương",
                    $"Lương tháng {bangLuong.Thang} đã được thanh toán: {bangLuong.TongThanhToan:N0} VNĐ",
                    "APP"
                );

                // Send email if available
                if (!string.IsNullOrEmpty(trainer.Email))
                {
                    var subject = $"Xác nhận thanh toán lương tháng {bangLuong.Thang}";
                    var body = $@"
                        <html>
                        <body>
                            <h2>Xác nhận thanh toán lương</h2>
                            <p>Xin chào {trainer.Ho} {trainer.Ten},</p>
                            <p>Lương tháng {bangLuong.Thang} của bạn đã được thanh toán:</p>
                            <ul>
                                <li>Lương cơ bản: {bangLuong.LuongCoBan:N0} VNĐ</li>
                                <li>Hoa hồng: {bangLuong.TienHoaHong:N0} VNĐ</li>
                                <li><strong>Tổng cộng: {bangLuong.TongThanhToan:N0} VNĐ</strong></li>
                            </ul>
                            <p>Ngày thanh toán: {bangLuong.NgayThanhToan:dd/MM/yyyy}</p>
                            <p>Trân trọng,<br/>Đội ngũ Gym Management</p>
                        </body>
                        </html>";

                    await _emailService.SendEmailAsync(trainer.Email, $"{trainer.Ho} {trainer.Ten}", subject, body);
                }
            }

            return true;
        }

        public async Task<bool> PayAllSalariesForMonthAsync(string thang)
        {
            var unpaidSalaries = await _bangLuongRepository.GetByMonthAsync(thang);
            var unpaidOnly = unpaidSalaries.Where(s => s.NgayThanhToan == null);

            foreach (var salary in unpaidOnly)
            {
                await PaySalaryAsync(salary.BangLuongId);
            }

            return true;
        }

        public async Task<decimal> CalculateCommissionAsync(int hlvId, string thang)
        {
            var result = await CalculateDetailedCommissionAsync(hlvId, thang);
            return result.TotalCommission;
        }

        public async Task<CommissionBreakdown> CalculateDetailedCommissionAsync(int hlvId, string thang)
        {
            // Parse month string (format: YYYY-MM)
            if (!DateTime.TryParseExact($"{thang}-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var monthStart))
                return new CommissionBreakdown();

            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var monthStartDateOnly = DateOnly.FromDateTime(monthStart);
            var monthEndDateOnly = DateOnly.FromDateTime(monthEnd);

            var breakdown = new CommissionBreakdown();

            // 1. Calculate Package Commission
            breakdown.PackageCommission = await CalculateBasicPackageCommissionAsync(hlvId, monthStart, monthEnd);

            // 2. Calculate Class Commission  
            breakdown.ClassCommission = await CalculateClassCommissionAsync(hlvId, monthStart, monthEnd);

            // 3. Calculate Personal Training Commission
            breakdown.PersonalCommission = await CalculatePersonalTrainingCommissionAsync(hlvId, monthStartDateOnly, monthEndDateOnly);

            // 4. Calculate Performance Bonuses
            breakdown.PerformanceBonus = await CalculatePerformanceBonusAsync(hlvId, monthStartDateOnly, monthEndDateOnly);

            // 5. Calculate Attendance Bonus
            breakdown.AttendanceBonus = await CalculateAttendanceBonusAsync(hlvId, monthStartDateOnly, monthEndDateOnly);

            // 6. Calculate metrics for reporting
            await PopulatePerformanceMetricsAsync(breakdown, hlvId, monthStartDateOnly, monthEndDateOnly);

            return breakdown;
        }

        private async Task<decimal> CalculateBasicPackageCommissionAsync(int hlvId, DateTime monthStart, DateTime monthEnd)
        {
            decimal totalCommission = 0;

            // Get commission configurations for packages
            var commissionConfigs = await _unitOfWork.Context.CauHinhHoaHongs
                .Include(c => c.GoiTap)
                .Where(c => c.GoiTapId.HasValue && c.KichHoat)
                .ToListAsync();

            // Get successful package registrations for classes taught by this trainer
            var packageRegistrations = await _unitOfWork.Context.DangKys
                .Include(d => d.GoiTap)
                .Include(d => d.LopHoc)
                .Include(d => d.ThanhToans)
                .Where(d => d.NgayTao >= monthStart && d.NgayTao <= monthEnd)
                .Where(d => d.GoiTapId.HasValue && d.LopHocId.HasValue)
                .Where(d => d.LopHoc.HlvId == hlvId) // Only for this trainer's classes
                .Where(d => d.ThanhToans.Any(t => t.TrangThai == "SUCCESS"))
                .ToListAsync();

            foreach (var registration in packageRegistrations)
            {
                var config = commissionConfigs.FirstOrDefault(c => c.GoiTapId == registration.GoiTapId);
                if (config != null && registration.GoiTap != null)
                {
                    var packagePrice = registration.GoiTap.Gia;
                    var commissionRate = config.PhanTramHoaHong / 100m;
                    totalCommission += packagePrice * commissionRate;
                }
            }

            return totalCommission;
        }

        private async Task<decimal> CalculateClassCommissionAsync(int hlvId, DateTime monthStart, DateTime monthEnd)
        {
            // Get commission configurations for classes
            var commissionConfigs = await _unitOfWork.Context.CauHinhHoaHongs
                .Include(c => c.LopHoc)
                .Where(c => c.LopHocId.HasValue && c.KichHoat)
                .ToListAsync();

            decimal totalCommission = 0;

            // Get successful class registrations for this trainer's classes
            var classRegistrations = await _unitOfWork.Context.DangKys
                .Include(d => d.LopHoc)
                .Include(d => d.ThanhToans)
                .Where(d => d.NgayTao >= monthStart && d.NgayTao <= monthEnd)
                .Where(d => d.LopHocId.HasValue && d.LopHoc.HlvId == hlvId)
                .Where(d => d.ThanhToans.Any(t => t.TrangThai == "SUCCESS"))
                .ToListAsync();

            foreach (var registration in classRegistrations)
            {
                var config = commissionConfigs.FirstOrDefault(c => c.LopHocId == registration.LopHocId);
                if (config == null)
                {
                    // Use default class commission if no specific config
                    config = commissionConfigs.FirstOrDefault(c => c.LopHocId == null);
                }

                if (config != null && registration.LopHoc != null)
                {
                    var classPrice = registration.LopHoc.GiaTuyChinh ?? 500000; // Default price
                    var commissionRate = config.PhanTramHoaHong / 100m;
                    totalCommission += classPrice * commissionRate;
                }
            }

            return totalCommission;
        }

        private async Task<decimal> CalculatePersonalTrainingCommissionAsync(int hlvId, DateOnly monthStart, DateOnly monthEnd)
        {
            // Simple default commission for personal training
            decimal baseCommissionPerSession = 50000; // 50k VND per session

            var personalSessions = await _unitOfWork.Context.BuoiHlvs
                .Where(b => b.HlvId == hlvId && b.NgayTap >= monthStart && b.NgayTap <= monthEnd)
                .ToListAsync();

            return personalSessions.Count() * baseCommissionPerSession;
        }

        private async Task<decimal> CalculatePerformanceBonusAsync(int hlvId, DateOnly monthStart, DateOnly monthEnd)
        {
            // Simple performance bonus based on student count
            var studentCount = await _unitOfWork.Context.DangKys
                .Include(d => d.LopHoc)
                .Where(d => d.NgayBatDau >= monthStart && d.NgayBatDau <= monthEnd)
                .Where(d => d.LopHocId.HasValue && d.LopHoc.HlvId == hlvId)
                .Where(d => d.TrangThai == "ACTIVE")
                .CountAsync();

            // Simple performance bonus tiers
            if (studentCount >= 50) return 1000000; // 1M for 50+ students
            if (studentCount >= 30) return 500000;  // 500k for 30+ students  
            if (studentCount >= 20) return 200000;  // 200k for 20+ students

            return 0;
        }

        private async Task<decimal> CalculateAttendanceBonusAsync(int hlvId, DateOnly monthStart, DateOnly monthEnd)
        {
            // Calculate average attendance rate for trainer's classes
            var trainerClasses = await _unitOfWork.Context.LichLops
                .Include(l => l.LopHoc)
                .Where(l => l.Ngay >= monthStart && l.Ngay <= monthEnd)
                .Where(l => l.LopHoc.HlvId == hlvId)
                .ToListAsync();

            if (!trainerClasses.Any()) return 0;

            var totalSlots = trainerClasses.Sum(c => c.LopHoc.SucChua);
            var totalAttendance = await _unitOfWork.Context.DiemDanhs
                .Where(d => d.ThoiGian >= monthStart.ToDateTime(TimeOnly.MinValue) && 
                           d.ThoiGian <= monthEnd.ToDateTime(TimeOnly.MaxValue))
                .Where(d => d.LichLop != null && trainerClasses.Select(tc => tc.LichLopId).Contains(d.LichLopId.Value))
                .Where(d => d.TrangThai == "Present")
                .CountAsync();

            var attendanceRate = totalSlots > 0 ? (decimal)totalAttendance / totalSlots : 0;

            // Attendance bonus tiers
            if (attendanceRate >= 0.9m) return 500000; // 90%+ attendance
            if (attendanceRate >= 0.8m) return 300000; // 80%+ attendance
            if (attendanceRate >= 0.7m) return 100000; // 70%+ attendance

            return 0;
        }

        private async Task PopulatePerformanceMetricsAsync(CommissionBreakdown breakdown, int hlvId, DateOnly monthStart, DateOnly monthEnd)
        {
            // Count students
            breakdown.StudentCount = await _unitOfWork.Context.DangKys
                .Include(d => d.LopHoc)
                .Where(d => d.NgayBatDau >= monthStart && d.NgayBatDau <= monthEnd)
                .Where(d => d.LopHocId.HasValue && d.LopHoc.HlvId == hlvId)
                .Where(d => d.TrangThai == "ACTIVE")
                .CountAsync();

            // Count classes taught
            breakdown.ClassesTaught = await _unitOfWork.Context.LichLops
                .Include(l => l.LopHoc)
                .Where(l => l.Ngay >= monthStart && l.Ngay <= monthEnd)
                .Where(l => l.LopHoc.HlvId == hlvId)
                .CountAsync();

            // Count personal sessions
            breakdown.PersonalSessions = await _unitOfWork.Context.BuoiHlvs
                .Where(b => b.HlvId == hlvId && b.NgayTap >= monthStart && b.NgayTap <= monthEnd)
                .CountAsync();

            // Calculate attendance rate
            var trainerClasses = await _unitOfWork.Context.LichLops
                .Include(l => l.LopHoc)
                .Where(l => l.Ngay >= monthStart && l.Ngay <= monthEnd)
                .Where(l => l.LopHoc.HlvId == hlvId)
                .ToListAsync();

            if (trainerClasses.Any())
            {
                var totalSlots = trainerClasses.Sum(c => c.LopHoc.SucChua);
                var totalAttendance = await _unitOfWork.Context.DiemDanhs
                    .Where(d => d.ThoiGian >= monthStart.ToDateTime(TimeOnly.MinValue) && 
                               d.ThoiGian <= monthEnd.ToDateTime(TimeOnly.MaxValue))
                    .Where(d => d.LichLop != null && trainerClasses.Select(tc => tc.LichLopId).Contains(d.LichLopId.Value))
                    .Where(d => d.TrangThai == "Present")
                    .CountAsync();

                breakdown.AttendanceRate = totalSlots > 0 ? (decimal)totalAttendance / totalSlots * 100 : 0;
            }
        }

        // Helper class for commission breakdown
        public class CommissionBreakdown
        {
            public decimal PackageCommission { get; set; }
            public decimal ClassCommission { get; set; }
            public decimal PersonalCommission { get; set; }
            public decimal PerformanceBonus { get; set; }
            public decimal AttendanceBonus { get; set; }
            public decimal TotalCommission => PackageCommission + ClassCommission + PersonalCommission + PerformanceBonus + AttendanceBonus;
            
            // Performance metrics
            public int StudentCount { get; set; }
            public int ClassesTaught { get; set; }
            public int PersonalSessions { get; set; }
            public decimal AttendanceRate { get; set; }
        }

        public async Task<decimal> GetTotalSalaryExpenseAsync(string thang)
        {
            var salaries = await _bangLuongRepository.GetByMonthAsync(thang);
            return salaries.Sum(s => s.TongThanhToan);
        }
    }
}
