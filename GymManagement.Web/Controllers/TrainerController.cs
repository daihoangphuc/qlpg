using GymManagement.Web.Data.Models;
using GymManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GymManagement.Web.Controllers
{
    [Authorize(Roles = "Trainer")]
    public class TrainerController : Controller
    {
        private readonly ILopHocService _lopHocService;
        private readonly IBangLuongService _bangLuongService;
        private readonly INguoiDungService _nguoiDungService;
        private readonly IDiemDanhService _diemDanhService;
        private readonly IBaoCaoService _baoCaoService;
        private readonly IAuthService _authService;
        private readonly ILogger<TrainerController> _logger;

        public TrainerController(
            ILopHocService lopHocService,
            IBangLuongService bangLuongService,
            INguoiDungService nguoiDungService,
            IDiemDanhService diemDanhService,
            IBaoCaoService baoCaoService,
            IAuthService authService,
            ILogger<TrainerController> logger)
        {
            _lopHocService = lopHocService;
            _bangLuongService = bangLuongService;
            _nguoiDungService = nguoiDungService;
            _diemDanhService = diemDanhService;
            _baoCaoService = baoCaoService;
            _authService = authService;
            _logger = logger;
        }

        // Helper method to get current user
        private async Task<TaiKhoan?> GetCurrentUserAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return null;

            return await _authService.GetUserByIdAsync(userId);
        }

        // Dashboard - Trang chủ của Trainer
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user?.NguoiDungId == null)
                {
                    _logger.LogWarning("Trainer user not found or NguoiDungId is null");
                    return RedirectToAction("Login", "Auth");
                }

                var trainerId = user.NguoiDungId.Value;

                // Lấy thông tin trainer
                var trainer = await _nguoiDungService.GetByIdAsync(trainerId);
                ViewBag.Trainer = trainer;

                // Lấy lớp học được phân công
                var myClasses = await _lopHocService.GetClassesByTrainerAsync(trainerId);
                ViewBag.MyClasses = myClasses.Take(5); // Hiển thị 5 lớp gần nhất

                // Lấy thông tin lương tháng hiện tại
                var currentMonth = DateTime.Now.ToString("yyyy-MM");
                var currentSalary = await _bangLuongService.GetByTrainerAndMonthAsync(trainerId, currentMonth);
                ViewBag.CurrentSalary = currentSalary;

                // Thống kê cơ bản
                ViewBag.TotalClasses = myClasses.Count();
                ViewBag.ActiveClasses = myClasses.Count(c => c.TrangThai == "OPEN");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading trainer dashboard for user {UserId}", User.Identity?.Name);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dashboard.";
                return View();
            }
        }

        // Lớp của tôi - Danh sách lớp học được phân công
        public async Task<IActionResult> MyClasses()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user?.NguoiDungId == null)
                {
                    _logger.LogWarning("Trainer user not found or NguoiDungId is null");
                    return RedirectToAction("Login", "Auth");
                }

                var trainerId = user.NguoiDungId.Value;
                var myClasses = await _lopHocService.GetClassesByTrainerAsync(trainerId);

                return View(myClasses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading trainer classes for user {UserId}", User.Identity?.Name);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách lớp học.";
                return View(new List<LopHoc>());
            }
        }

        // Lịch dạy - Lịch dạy cá nhân
        public async Task<IActionResult> Schedule()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user?.NguoiDungId == null)
                {
                    _logger.LogWarning("Trainer user not found or NguoiDungId is null");
                    return RedirectToAction("Login", "Auth");
                }

                var trainerId = user.NguoiDungId.Value;
                
                // Lấy lớp học của trainer
                var myClasses = await _lopHocService.GetClassesByTrainerAsync(trainerId);
                ViewBag.MyClasses = myClasses;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading trainer schedule for user {UserId}", User.Identity?.Name);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải lịch dạy.";
                return View();
            }
        }

        // API để lấy lịch dạy dạng JSON cho calendar
        [HttpGet]
        public async Task<IActionResult> GetScheduleEvents(DateTime start, DateTime end)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user?.NguoiDungId == null)
                {
                    return Json(new List<object>());
                }

                var trainerId = user.NguoiDungId.Value;
                var myClasses = await _lopHocService.GetClassesByTrainerAsync(trainerId);

                var events = new List<object>();
                
                foreach (var lopHoc in myClasses)
                {
                    var schedules = await _lopHocService.GetClassScheduleAsync(lopHoc.LopHocId, start, end);
                    
                    foreach (var schedule in schedules)
                    {
                        events.Add(new
                        {
                            id = schedule.LichLopId,
                            title = lopHoc.TenLop,
                            start = schedule.Ngay.ToDateTime(schedule.GioBatDau),
                            end = schedule.Ngay.ToDateTime(schedule.GioKetThuc),
                            backgroundColor = schedule.TrangThai == "SCHEDULED" ? "#3b82f6" : "#ef4444",
                            borderColor = schedule.TrangThai == "SCHEDULED" ? "#2563eb" : "#dc2626",
                            extendedProps = new
                            {
                                status = schedule.TrangThai,
                                capacity = lopHoc.SucChua,
                                booked = schedule.SoLuongDaDat
                            }
                        });
                    }
                }

                return Json(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting trainer schedule events");
                return Json(new List<object>());
            }
        }

        // Học viên - Danh sách học viên trong các lớp của trainer
        public async Task<IActionResult> Students()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user?.NguoiDungId == null)
                {
                    _logger.LogWarning("Trainer user not found or NguoiDungId is null");
                    return RedirectToAction("Login", "Auth");
                }

                var trainerId = user.NguoiDungId.Value;

                // Lấy lớp học của trainer
                var myClasses = await _lopHocService.GetClassesByTrainerAsync(trainerId);
                ViewBag.MyClasses = myClasses;

                // Lấy danh sách học viên từ các lớp học (sẽ được load qua AJAX)
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading trainer students for user {UserId}", User.Identity?.Name);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách học viên.";
                return View();
            }
        }

        // API để lấy danh sách học viên theo lớp học
        [HttpGet]
        public async Task<IActionResult> GetStudentsByClass(int classId)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user?.NguoiDungId == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin trainer." });
                }

                var trainerId = user.NguoiDungId.Value;

                // Kiểm tra xem lớp học có thuộc về trainer này không
                var myClasses = await _lopHocService.GetClassesByTrainerAsync(trainerId);
                if (!myClasses.Any(c => c.LopHocId == classId))
                {
                    return Json(new { success = false, message = "Bạn không có quyền xem học viên của lớp này." });
                }

                // Lấy danh sách học viên (thông qua đăng ký)
                // Tạm thời return empty list, sẽ implement sau khi có DangKyService
                var students = new List<object>();

                return Json(new { success = true, data = students });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting students for class {ClassId}", classId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách học viên." });
            }
        }

        // Lương - Thông tin lương và hoa hồng
        public async Task<IActionResult> Salary()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user?.NguoiDungId == null)
                {
                    _logger.LogWarning("Trainer user not found or NguoiDungId is null");
                    return RedirectToAction("Login", "Auth");
                }

                var trainerId = user.NguoiDungId.Value;

                // Lấy lịch sử lương
                var salaries = await _bangLuongService.GetByTrainerIdAsync(trainerId);

                // Lấy thông tin lương tháng hiện tại
                var currentMonth = DateTime.Now.ToString("yyyy-MM");
                var currentSalary = await _bangLuongService.GetByTrainerAndMonthAsync(trainerId, currentMonth);
                ViewBag.CurrentSalary = currentSalary;

                // Tính tổng lương đã nhận
                ViewBag.TotalPaidSalary = salaries.Where(s => s.NgayThanhToan != null).Sum(s => s.TongThanhToan);
                ViewBag.PendingSalary = salaries.Where(s => s.NgayThanhToan == null).Sum(s => s.TongThanhToan);

                return View(salaries.OrderByDescending(s => s.Thang));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading trainer salary for user {UserId}", User.Identity?.Name);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin lương.";
                return View(new List<BangLuong>());
            }
        }

        // API để lấy chi tiết lương theo tháng
        [HttpGet]
        public async Task<IActionResult> GetSalaryDetails(string month)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user?.NguoiDungId == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin trainer." });
                }

                var trainerId = user.NguoiDungId.Value;
                var salary = await _bangLuongService.GetByTrainerAndMonthAsync(trainerId, month);

                if (salary == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin lương cho tháng này." });
                }

                // Tính hoa hồng chi tiết
                var commission = await _bangLuongService.CalculateCommissionAsync(trainerId, month);

                var result = new
                {
                    success = true,
                    data = new
                    {
                        month = salary.Thang,
                        baseSalary = salary.LuongCoBan,
                        commission = salary.TienHoaHong,
                        calculatedCommission = commission,
                        total = salary.TongThanhToan,
                        paymentDate = salary.NgayThanhToan?.ToString("dd/MM/yyyy"),
                        isPaid = salary.NgayThanhToan != null,
                        note = salary.GhiChu
                    }
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting salary details for month {Month}", month);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải chi tiết lương." });
            }
        }
    }
}
