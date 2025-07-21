using GymManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class BaoCaoController : Controller
    {
        private readonly IBaoCaoService _baoCaoService;
        private readonly ILogger<BaoCaoController> _logger;

        public BaoCaoController(IBaoCaoService baoCaoService, ILogger<BaoCaoController> logger)
        {
            _baoCaoService = baoCaoService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardData = await _baoCaoService.GetDashboardDataAsync();
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting dashboard data");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dữ liệu dashboard.";
                return View(new { });
            }
        }

        public IActionResult Revenue()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetRevenueData(DateTime startDate, DateTime endDate, string groupBy = "day")
        {
            try
            {
                var data = await _baoCaoService.GetRevenueByDateRangeAsync(startDate, endDate);
                return Json(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting revenue data");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải dữ liệu doanh thu." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRevenueByPaymentMethod(DateTime startDate, DateTime endDate)
        {
            try
            {
                var data = await _baoCaoService.GetRevenueByPaymentMethodAsync(startDate, endDate);
                return Json(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting revenue by payment method");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải dữ liệu doanh thu theo phương thức." });
            }
        }

        public IActionResult Membership()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetMembershipData()
        {
            try
            {
                var totalActive = await _baoCaoService.GetTotalActiveMembersAsync();
                var membersByPackage = await _baoCaoService.GetMembersByPackageAsync();
                var registrationTrend = await _baoCaoService.GetMemberRegistrationTrendAsync(12);

                return Json(new { 
                    success = true, 
                    totalActive = totalActive,
                    membersByPackage = membersByPackage,
                    registrationTrend = registrationTrend
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting membership data");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải dữ liệu thành viên." });
            }
        }

        public IActionResult Attendance()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAttendanceData(DateTime startDate, DateTime endDate)
        {
            try
            {
                var attendanceTrend = await _baoCaoService.GetAttendanceTrendAsync(startDate, endDate);
                var averageAttendance = await _baoCaoService.GetAverageAttendanceAsync(startDate, endDate);
                var todayAttendanceByTimeSlot = await _baoCaoService.GetAttendanceByTimeSlotAsync(DateTime.Today);

                return Json(new { 
                    success = true, 
                    attendanceTrend = attendanceTrend,
                    averageAttendance = averageAttendance,
                    todayByTimeSlot = todayAttendanceByTimeSlot
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting attendance data");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải dữ liệu điểm danh." });
            }
        }

        public IActionResult Classes()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetClassData(DateTime startDate, DateTime endDate)
        {
            try
            {
                var popularClasses = await _baoCaoService.GetPopularClassesAsync(startDate, endDate);
                var occupancyRates = await _baoCaoService.GetClassOccupancyRatesAsync(startDate, endDate);
                var cancellationRates = await _baoCaoService.GetClassCancellationRatesAsync(startDate, endDate);

                return Json(new { 
                    success = true, 
                    popularClasses = popularClasses,
                    occupancyRates = occupancyRates,
                    cancellationRates = cancellationRates
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting class data");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải dữ liệu lớp học." });
            }
        }

        public IActionResult Trainers()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTrainerData(DateTime startDate, DateTime endDate)
        {
            try
            {
                var trainerRevenue = await _baoCaoService.GetTrainerRevenueAsync(startDate, endDate);
                var trainerClassCount = await _baoCaoService.GetTrainerClassCountAsync(startDate, endDate);
                var currentMonth = DateTime.Now.ToString("yyyy-MM");
                var trainerCommission = await _baoCaoService.GetTrainerCommissionAsync(currentMonth);

                return Json(new { 
                    success = true, 
                    trainerRevenue = trainerRevenue,
                    trainerClassCount = trainerClassCount,
                    trainerCommission = trainerCommission
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting trainer data");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải dữ liệu huấn luyện viên." });
            }
        }

        public IActionResult Financial()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetFinancialData(int year, int month)
        {
            try
            {
                var monthlySummary = await _baoCaoService.GetMonthlyFinancialSummaryAsync(year, month);
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);
                var netProfit = await _baoCaoService.GetNetProfitAsync(startDate, endDate);

                return Json(new { 
                    success = true, 
                    monthlySummary = monthlySummary,
                    netProfit = netProfit
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting financial data");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải dữ liệu tài chính." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRealtimeStats()
        {
            try
            {
                var stats = await _baoCaoService.GetRealtimeStatsAsync();
                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting realtime stats");
                return Json(new { currentAttendance = 0, todayRevenue = 0, activeMembers = 0, lastUpdated = DateTime.Now });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportReport(string reportType, DateTime startDate, DateTime endDate, string format = "csv")
        {
            try
            {
                string csv = "";
                string filename = "";

                switch (reportType.ToLower())
                {
                    case "revenue":
                        var revenueData = await _baoCaoService.GetRevenueByDateRangeAsync(startDate, endDate);
                        csv = "Ngày,Doanh thu\n";
                        foreach (var item in revenueData)
                        {
                            csv += $"{item.Key},{item.Value}\n";
                        }
                        filename = $"DoanhThu_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv";
                        break;

                    case "attendance":
                        var attendanceData = await _baoCaoService.GetAttendanceTrendAsync(startDate, endDate);
                        csv = "Ngày,Số lượng điểm danh\n";
                        foreach (var item in attendanceData)
                        {
                            csv += $"{item.Key},{item.Value}\n";
                        }
                        filename = $"DiemDanh_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv";
                        break;

                    case "membership":
                        var membershipData = await _baoCaoService.GetMembersByPackageAsync();
                        csv = "Gói tập,Số thành viên\n";
                        foreach (var item in membershipData)
                        {
                            csv += $"{item.Key},{item.Value}\n";
                        }
                        filename = $"ThanhVien_{DateTime.Now:yyyyMMdd}.csv";
                        break;

                    default:
                        return BadRequest("Loại báo cáo không được hỗ trợ.");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                return File(bytes, "text/csv", filename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting report");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xuất báo cáo.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetQuickStats()
        {
            try
            {
                var today = DateTime.Today;
                var thisMonth = new DateTime(today.Year, today.Month, 1);

                var todayRevenue = await _baoCaoService.GetDailyRevenueAsync(today);
                var monthlyRevenue = await _baoCaoService.GetMonthlyRevenueAsync(today.Year, today.Month);
                var totalMembers = await _baoCaoService.GetTotalActiveMembersAsync();
                var todayAttendance = await _baoCaoService.GetDailyAttendanceAsync(today);

                return Json(new {
                    success = true,
                    todayRevenue = todayRevenue,
                    monthlyRevenue = monthlyRevenue,
                    totalMembers = totalMembers,
                    todayAttendance = todayAttendance
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting quick stats");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải thống kê nhanh." });
            }
        }

        public IActionResult CustomReport()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateCustomReport(string reportName, DateTime startDate, DateTime endDate, string[] metrics)
        {
            try
            {
                var reportData = new Dictionary<string, object>();

                foreach (var metric in metrics)
                {
                    switch (metric.ToLower())
                    {
                        case "revenue":
                            reportData["revenue"] = await _baoCaoService.GetRevenueByDateRangeAsync(startDate, endDate);
                            break;
                        case "attendance":
                            reportData["attendance"] = await _baoCaoService.GetAttendanceTrendAsync(startDate, endDate);
                            break;
                        case "membership":
                            reportData["membership"] = await _baoCaoService.GetMembersByPackageAsync();
                            break;
                        case "classes":
                            reportData["classes"] = await _baoCaoService.GetPopularClassesAsync(startDate, endDate);
                            break;
                    }
                }

                ViewBag.ReportName = reportName;
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                ViewBag.ReportData = reportData;

                return View("CustomReportResult");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating custom report");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo báo cáo tùy chỉnh.";
                return View();
            }
        }
    }
}
