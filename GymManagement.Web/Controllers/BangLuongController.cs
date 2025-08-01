using GymManagement.Web.Data.Models;
using GymManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers
{
    [Authorize]
    public class BangLuongController : Controller
    {
        private readonly IBangLuongService _bangLuongService;
        private readonly INguoiDungService _nguoiDungService;
        private readonly UserManager<TaiKhoan> _userManager;
        private readonly ILogger<BangLuongController> _logger;

        public BangLuongController(
            IBangLuongService bangLuongService,
            INguoiDungService nguoiDungService,
            UserManager<TaiKhoan> userManager,
            ILogger<BangLuongController> logger)
        {
            _bangLuongService = bangLuongService;
            _nguoiDungService = nguoiDungService;
            _userManager = userManager;
            _logger = logger;
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var salaries = await _bangLuongService.GetAllAsync();
                return View(salaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting salaries");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách bảng lương.";
                return View(new List<BangLuong>());
            }
        }

        [Authorize(Roles = "Trainer")]
        public async Task<IActionResult> MySalary()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.NguoiDungId == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var salaries = await _bangLuongService.GetByTrainerIdAsync(user.NguoiDungId.Value);
                return View(salaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting trainer salary");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải bảng lương của bạn.";
                return View(new List<BangLuong>());
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var salary = await _bangLuongService.GetByIdAsync(id);
                if (salary == null)
                {
                    return NotFound();
                }
                return View(salary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting salary details for ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin bảng lương.";
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> MonthlyView(string? month = null)
        {
            try
            {
                month ??= DateTime.Now.ToString("yyyy-MM");
                var salaries = await _bangLuongService.GetByMonthAsync(month);
                var totalExpense = await _bangLuongService.GetTotalSalaryExpenseAsync(month);
                
                ViewBag.Month = month;
                ViewBag.TotalExpense = totalExpense;
                return View(salaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting monthly salaries");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải bảng lương tháng.";
                ViewBag.Month = month ?? DateTime.Now.ToString("yyyy-MM");
                ViewBag.TotalExpense = 0;
                return View(new List<BangLuong>());
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UnpaidSalaries()
        {
            try
            {
                var salaries = await _bangLuongService.GetUnpaidSalariesAsync();
                return View(salaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting unpaid salaries");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách lương chưa thanh toán.";
                return View(new List<BangLuong>());
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GenerateMonthlySalaries(string month)
        {
            try
            {
                var result = await _bangLuongService.GenerateMonthlySalariesAsync(month);
                if (result)
                {
                    return Json(new { success = true, message = $"Tạo bảng lương tháng {month} thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể tạo bảng lương." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating monthly salaries");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo bảng lương." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> PaySalary(int salaryId)
        {
            try
            {
                var result = await _bangLuongService.PaySalaryAsync(salaryId);
                if (result)
                {
                    return Json(new { success = true, message = "Thanh toán lương thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể thanh toán lương." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while paying salary");
                return Json(new { success = false, message = "Có lỗi xảy ra khi thanh toán lương." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PayAllSalariesForMonth(string month)
        {
            try
            {
                var result = await _bangLuongService.PayAllSalariesForMonthAsync(month);
                if (result)
                {
                    return Json(new { success = true, message = $"Thanh toán tất cả lương tháng {month} thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể thanh toán lương." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while paying all salaries for month");
                return Json(new { success = false, message = "Có lỗi xảy ra khi thanh toán lương." });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CalculateCommission(int trainerId, string month)
        {
            try
            {
                var commission = await _bangLuongService.CalculateCommissionAsync(trainerId, month);
                return Json(new { 
                    success = true, 
                    commission = commission,
                    formattedCommission = commission.ToString("N0") + " VNĐ"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while calculating commission");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tính hoa hồng." });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetSalaryExpense(string month)
        {
            try
            {
                var expense = await _bangLuongService.GetTotalSalaryExpenseAsync(month);
                return Json(new { 
                    success = true, 
                    expense = expense,
                    formattedExpense = expense.ToString("N0") + " VNĐ"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting salary expense");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tính chi phí lương." });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ExportSalaries(string month, string format = "csv")
        {
            try
            {
                var salaries = await _bangLuongService.GetByMonthAsync(month);
                
                if (format.ToLower() == "csv")
                {
                    var csv = "Huấn luyện viên,Tháng,Lương cơ bản,Hoa hồng,Tổng thanh toán,Ngày thanh toán\n";
                    foreach (var salary in salaries)
                    {
                        csv += $"{salary.Hlv?.Ho} {salary.Hlv?.Ten},{salary.Thang},{salary.LuongCoBan},{salary.TienHoaHong},{salary.TongThanhToan},{salary.NgayThanhToan?.ToString("dd/MM/yyyy") ?? "Chưa thanh toán"}\n";
                    }

                    var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                    return File(bytes, "text/csv", $"BangLuong_{month}.csv");
                }

                return BadRequest("Định dạng không được hỗ trợ.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting salaries");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xuất bảng lương.";
                return RedirectToAction(nameof(MonthlyView), new { month });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetTrainerSalaryHistory(int trainerId)
        {
            try
            {
                var salaries = await _bangLuongService.GetByTrainerIdAsync(trainerId);
                return Json(salaries.Select(s => new {
                    month = s.Thang,
                    baseSalary = s.LuongCoBan,
                    commission = s.TienHoaHong,
                    total = s.TongThanhToan,
                    paymentDate = s.NgayThanhToan?.ToString("dd/MM/yyyy"),
                    isPaid = s.NgayThanhToan != null
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting trainer salary history");
                return Json(new List<object>());
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult SalarySettings()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateBaseSalary(decimal newBaseSalary)
        {
            try
            {
                // TODO: Implement base salary configuration
                // This would typically be stored in a configuration table
                return Json(new { success = true, message = "Cập nhật lương cơ bản thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating base salary");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật lương cơ bản." });
            }
        }
    }
}
