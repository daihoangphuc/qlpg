using GymManagement.Web.Data;
using GymManagement.Web.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GymManagement.Web.Services
{
    public class BaoCaoService : IBaoCaoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IThanhToanRepository _thanhToanRepository;
        private readonly IDangKyRepository _dangKyRepository;
        private readonly IDiemDanhRepository _diemDanhRepository;
        private readonly IBangLuongRepository _bangLuongRepository;
        private readonly IMemoryCache _cache;

        public BaoCaoService(
            IUnitOfWork unitOfWork,
            IThanhToanRepository thanhToanRepository,
            IDangKyRepository dangKyRepository,
            IDiemDanhRepository diemDanhRepository,
            IBangLuongRepository bangLuongRepository,
            IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _thanhToanRepository = thanhToanRepository;
            _dangKyRepository = dangKyRepository;
            _diemDanhRepository = diemDanhRepository;
            _bangLuongRepository = bangLuongRepository;
            _cache = cache;
        }

        public async Task<decimal> GetDailyRevenueAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);
            return await _thanhToanRepository.GetTotalRevenueByDateRangeAsync(startDate, endDate);
        }

        public async Task<decimal> GetMonthlyRevenueAsync(int year, int month)
        {
            return await _thanhToanRepository.GetTotalRevenueByMonthAsync(year, month);
        }

        public async Task<decimal> GetYearlyRevenueAsync(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);
            return await _thanhToanRepository.GetTotalRevenueByDateRangeAsync(startDate, endDate);
        }

        public async Task<Dictionary<string, decimal>> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var payments = await _thanhToanRepository.GetPaymentsByDateRangeAsync(startDate, endDate);
            var successfulPayments = payments.Where(p => p.TrangThai == "SUCCESS");

            var result = new Dictionary<string, decimal>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var dayRevenue = successfulPayments
                    .Where(p => p.NgayThanhToan.Date == currentDate)
                    .Sum(p => p.SoTien);

                result[currentDate.ToString("yyyy-MM-dd")] = dayRevenue;
                currentDate = currentDate.AddDays(1);
            }

            return result;
        }

        public async Task<Dictionary<string, decimal>> GetRevenueByPaymentMethodAsync(DateTime startDate, DateTime endDate)
        {
            var payments = await _thanhToanRepository.GetPaymentsByDateRangeAsync(startDate, endDate);
            var successfulPayments = payments.Where(p => p.TrangThai == "SUCCESS");

            return successfulPayments
                .GroupBy(p => p.PhuongThuc ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Sum(p => p.SoTien));
        }

        public async Task<int> GetTotalActiveMembersAsync()
        {
            return await _dangKyRepository.CountActiveRegistrationsAsync();
        }

        public async Task<int> GetNewMembersCountAsync(DateTime startDate, DateTime endDate)
        {
            var registrations = await _dangKyRepository.GetRegistrationsByDateRangeAsync(startDate, endDate);
            return registrations.Count();
        }

        public async Task<int> GetExpiredMembersCountAsync(DateTime startDate, DateTime endDate)
        {
            var expiredRegistrations = await _dangKyRepository.GetExpiredRegistrationsAsync();
            return expiredRegistrations.Count(r => r.NgayKetThuc >= startDate && r.NgayKetThuc <= endDate);
        }

        public async Task<Dictionary<string, int>> GetMembersByPackageAsync()
        {
            var activeRegistrations = await _dangKyRepository.GetActiveRegistrationsAsync();
            
            return activeRegistrations
                .Where(r => r.GoiTap != null)
                .GroupBy(r => r.GoiTap!.TenGoi)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetMemberRegistrationTrendAsync(int months)
        {
            var result = new Dictionary<string, int>();
            var currentDate = DateTime.Today.AddMonths(-months + 1);

            for (int i = 0; i < months; i++)
            {
                var monthStart = new DateTime(currentDate.Year, currentDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var count = await GetNewMembersCountAsync(monthStart, monthEnd);
                result[currentDate.ToString("yyyy-MM")] = count;

                currentDate = currentDate.AddMonths(1);
            }

            return result;
        }

        public async Task<int> GetDailyAttendanceAsync(DateTime date)
        {
            return await _diemDanhRepository.CountAttendanceByDateAsync(date);
        }

        public async Task<Dictionary<string, int>> GetAttendanceTrendAsync(DateTime startDate, DateTime endDate)
        {
            var result = new Dictionary<string, int>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var count = await _diemDanhRepository.CountAttendanceByDateAsync(currentDate);
                result[currentDate.ToString("yyyy-MM-dd")] = count;
                currentDate = currentDate.AddDays(1);
            }

            return result;
        }

        public async Task<Dictionary<string, int>> GetAttendanceByTimeSlotAsync(DateTime date)
        {
            var attendance = await _diemDanhRepository.GetByDateRangeAsync(date, date.AddDays(1));
            
            var timeSlots = new Dictionary<string, int>
            {
                ["06:00-09:00"] = 0,
                ["09:00-12:00"] = 0,
                ["12:00-15:00"] = 0,
                ["15:00-18:00"] = 0,
                ["18:00-21:00"] = 0,
                ["21:00-24:00"] = 0
            };

            foreach (var record in attendance)
            {
                var hour = record.ThoiGian.Hour;
                var timeSlot = hour switch
                {
                    >= 6 and < 9 => "06:00-09:00",
                    >= 9 and < 12 => "09:00-12:00",
                    >= 12 and < 15 => "12:00-15:00",
                    >= 15 and < 18 => "15:00-18:00",
                    >= 18 and < 21 => "18:00-21:00",
                    >= 21 or < 6 => "21:00-24:00",
                    _ => "Unknown"
                };

                if (timeSlots.ContainsKey(timeSlot))
                    timeSlots[timeSlot]++;
            }

            return timeSlots;
        }

        public async Task<double> GetAverageAttendanceAsync(DateTime startDate, DateTime endDate)
        {
            var attendanceTrend = await GetAttendanceTrendAsync(startDate, endDate);
            return attendanceTrend.Values.Any() ? attendanceTrend.Values.Average() : 0;
        }

        public async Task<Dictionary<string, int>> GetPopularClassesAsync(DateTime startDate, DateTime endDate)
        {
            var bookings = await _unitOfWork.Context.Bookings
                .Include(b => b.LopHoc)
                .Where(b => b.Ngay >= startDate && b.Ngay <= endDate && b.TrangThai == "BOOKED")
                .ToListAsync();

            return bookings
                .Where(b => b.LopHoc != null)
                .GroupBy(b => b.LopHoc!.TenLop)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, double>> GetClassOccupancyRatesAsync(DateTime startDate, DateTime endDate)
        {
            var classes = await _unitOfWork.Context.LopHocs
                .Where(l => l.TrangThai == "OPEN")
                .ToListAsync();

            var result = new Dictionary<string, double>();

            foreach (var lopHoc in classes)
            {
                var totalBookings = await _unitOfWork.Context.Bookings
                    .CountAsync(b => b.LopHocId == lopHoc.LopHocId && 
                                    b.Ngay >= startDate && 
                                    b.Ngay <= endDate && 
                                    b.TrangThai == "BOOKED");

                var totalDays = (endDate - startDate).Days + 1;
                var maxCapacity = lopHoc.SucChua * totalDays;
                var occupancyRate = maxCapacity > 0 ? (double)totalBookings / maxCapacity * 100 : 0;

                result[lopHoc.TenLop] = Math.Round(occupancyRate, 2);
            }

            return result;
        }

        public async Task<Dictionary<string, int>> GetClassCancellationRatesAsync(DateTime startDate, DateTime endDate)
        {
            var schedules = await _unitOfWork.Context.LichLops
                .Include(l => l.LopHoc)
                .Where(l => l.Ngay >= startDate && l.Ngay <= endDate)
                .ToListAsync();

            return schedules
                .Where(s => s.LopHoc != null)
                .GroupBy(s => s.LopHoc!.TenLop)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count(s => s.TrangThai == "CANCELED")
                );
        }

        public async Task<Dictionary<string, decimal>> GetTrainerRevenueAsync(DateTime startDate, DateTime endDate)
        {
            var registrations = await _unitOfWork.Context.DangKys
                .Include(d => d.LopHoc)
                    .ThenInclude(l => l.Hlv)
                .Include(d => d.ThanhToans)
                .Where(d => d.NgayTao >= startDate && d.NgayTao <= endDate)
                .Where(d => d.LopHocId != null)
                .ToListAsync();

            return registrations
                .Where(r => r.LopHoc?.Hlv != null && r.ThanhToans.Any(t => t.TrangThai == "SUCCESS"))
                .GroupBy(r => $"{r.LopHoc!.Hlv!.Ho} {r.LopHoc.Hlv.Ten}")
                .ToDictionary(
                    g => g.Key,
                    g => g.SelectMany(r => r.ThanhToans.Where(t => t.TrangThai == "SUCCESS")).Sum(t => t.SoTien)
                );
        }

        public async Task<Dictionary<string, int>> GetTrainerClassCountAsync(DateTime startDate, DateTime endDate)
        {
            var schedules = await _unitOfWork.Context.LichLops
                .Include(l => l.LopHoc)
                    .ThenInclude(lh => lh.Hlv)
                .Where(l => l.Ngay >= startDate && l.Ngay <= endDate && l.TrangThai != "CANCELED")
                .ToListAsync();

            return schedules
                .Where(s => s.LopHoc?.Hlv != null)
                .GroupBy(s => $"{s.LopHoc!.Hlv!.Ho} {s.LopHoc.Hlv.Ten}")
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, decimal>> GetTrainerCommissionAsync(string thang)
        {
            var salaries = await _bangLuongRepository.GetByMonthAsync(thang);
            
            return salaries
                .Where(s => s.Hlv != null)
                .ToDictionary(
                    s => $"{s.Hlv!.Ho} {s.Hlv.Ten}",
                    s => s.TienHoaHong
                );
        }

        public async Task<Dictionary<string, decimal>> GetMonthlyFinancialSummaryAsync(int year, int month)
        {
            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var revenue = await GetMonthlyRevenueAsync(year, month);
            var salaryExpense = await _bangLuongRepository.GetTotalSalaryByMonthAsync($"{year:0000}-{month:00}");
            var commissionExpense = await _bangLuongRepository.GetTotalCommissionByMonthAsync($"{year:0000}-{month:00}");

            return new Dictionary<string, decimal>
            {
                ["Revenue"] = revenue,
                ["SalaryExpense"] = salaryExpense,
                ["CommissionExpense"] = commissionExpense,
                ["TotalExpense"] = salaryExpense + commissionExpense,
                ["NetProfit"] = revenue - (salaryExpense + commissionExpense)
            };
        }

        public async Task<decimal> GetTotalExpensesAsync(DateTime startDate, DateTime endDate)
        {
            // For simplicity, we'll calculate salary expenses for the months in the date range
            decimal totalExpenses = 0;
            var currentMonth = new DateTime(startDate.Year, startDate.Month, 1);
            var endMonth = new DateTime(endDate.Year, endDate.Month, 1);

            while (currentMonth <= endMonth)
            {
                var monthString = currentMonth.ToString("yyyy-MM");
                var salaryExpense = await _bangLuongRepository.GetTotalSalaryByMonthAsync(monthString);
                var commissionExpense = await _bangLuongRepository.GetTotalCommissionByMonthAsync(monthString);
                totalExpenses += salaryExpense + commissionExpense;

                currentMonth = currentMonth.AddMonths(1);
            }

            return totalExpenses;
        }

        public async Task<decimal> GetNetProfitAsync(DateTime startDate, DateTime endDate)
        {
            var revenue = await _thanhToanRepository.GetTotalRevenueByDateRangeAsync(startDate, endDate);
            var expenses = await GetTotalExpensesAsync(startDate, endDate);
            return revenue - expenses;
        }

        public async Task<object> GetDashboardDataAsync()
        {
            const string cacheKey = "dashboard_data";
            if (_cache.TryGetValue(cacheKey, out object? cachedData))
                return cachedData!;

            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);

            var data = new
            {
                TotalActiveMembers = await GetTotalActiveMembersAsync(),
                TodayAttendance = await GetDailyAttendanceAsync(today),
                TodayRevenue = await GetDailyRevenueAsync(today),
                ThisMonthRevenue = await GetMonthlyRevenueAsync(today.Year, today.Month),
                LastMonthRevenue = await GetMonthlyRevenueAsync(lastMonth.Year, lastMonth.Month),
                NewMembersThisMonth = await GetNewMembersCountAsync(thisMonth, today),
                PopularPackages = await GetMembersByPackageAsync(),
                AttendanceTrend = await GetAttendanceTrendAsync(today.AddDays(-7), today),
                RevenueTrend = await GetRevenueByDateRangeAsync(today.AddDays(-30), today)
            };

            _cache.Set(cacheKey, data, TimeSpan.FromMinutes(5));
            return data;
        }

        public async Task<object> GetRealtimeStatsAsync()
        {
            var today = DateTime.Today;
            
            return new
            {
                CurrentAttendance = await GetDailyAttendanceAsync(today),
                TodayRevenue = await GetDailyRevenueAsync(today),
                ActiveMembers = await GetTotalActiveMembersAsync(),
                LastUpdated = DateTime.Now
            };
        }
    }
}
