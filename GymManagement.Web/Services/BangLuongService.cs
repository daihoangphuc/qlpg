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

                // Calculate commission
                decimal commission = await CalculateCommissionAsync(trainer.NguoiDungId, thang);

                // Create salary record
                var bangLuong = new BangLuong
                {
                    HlvId = trainer.NguoiDungId,
                    Thang = thang,
                    LuongCoBan = baseSalary,
                    TienHoaHong = commission
                };

                await _bangLuongRepository.AddAsync(bangLuong);

                // Send notification
                await _thongBaoService.CreateNotificationAsync(
                    trainer.NguoiDungId,
                    "Bảng lương tháng mới",
                    $"Bảng lương tháng {thang} đã được tạo. Tổng: {(baseSalary + commission):N0} VNĐ",
                    "APP"
                );
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PaySalaryAsync(int bangLuongId)
        {
            var bangLuong = await _bangLuongRepository.GetByIdAsync(bangLuongId);
            if (bangLuong == null || bangLuong.NgayThanhToan != null) return false;

            bangLuong.NgayThanhToan = DateTime.Today;
            await _unitOfWork.SaveChangesAsync();

            // Send notifications
            var trainer = await _nguoiDungRepository.GetByIdAsync(bangLuong.HlvId);
            if (trainer != null)
            {
                await _thongBaoService.CreateNotificationAsync(
                    bangLuong.HlvId,
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
            // Parse month string (format: YYYY-MM)
            if (!DateTime.TryParseExact($"{thang}-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var monthStart))
                return 0;

            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // Get commission configurations
            var commissionConfigs = await _unitOfWork.Context.CauHinhHoaHongs
                .Include(c => c.GoiTap)
                .ToListAsync();

            decimal totalCommission = 0;

            // Calculate commission from successful registrations
            var registrations = await _unitOfWork.Context.DangKys
                .Include(d => d.GoiTap)
                .Include(d => d.LopHoc)
                .Include(d => d.ThanhToans)
                .Where(d => d.NgayTao >= monthStart && d.NgayTao <= monthEnd)
                .Where(d => d.LopHoc == null || d.LopHoc.HlvId == hlvId) // Only class registrations for this trainer
                .ToListAsync();

            foreach (var registration in registrations)
            {
                // Only count if payment was successful
                var hasSuccessfulPayment = registration.ThanhToans.Any(t => t.TrangThai == "SUCCESS");
                if (!hasSuccessfulPayment) continue;

                if (registration.GoiTapId.HasValue)
                {
                    var config = commissionConfigs.FirstOrDefault(c => c.GoiTapId == registration.GoiTapId);
                    if (config != null && registration.GoiTap != null)
                    {
                        var packagePrice = registration.GoiTap.Gia;
                        var commissionRate = config.PhanTramHoaHong / 100m;
                        totalCommission += packagePrice * commissionRate;
                    }
                }
            }

            // Calculate commission from personal training sessions
            var personalSessions = await _unitOfWork.Context.BuoiHlvs
                .Where(b => b.HlvId == hlvId && b.NgayTap >= monthStart && b.NgayTap <= monthEnd)
                .ToListAsync();

            // Assume 100,000 VND commission per personal training session
            totalCommission += personalSessions.Count * 100000;

            return totalCommission;
        }

        public async Task<decimal> GetTotalSalaryExpenseAsync(string thang)
        {
            var salaries = await _bangLuongRepository.GetByMonthAsync(thang);
            return salaries.Sum(s => s.TongThanhToan);
        }
    }
}
