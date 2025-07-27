using GymManagement.Web.Data.Models;
using GymManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace GymManagement.Web.Controllers
{
    [Authorize]
    public class DangKyController : Controller
    {
        private readonly IDangKyService _dangKyService;
        private readonly IGoiTapService _goiTapService;
        private readonly ILopHocService _lopHocService;
        private readonly INguoiDungService _nguoiDungService;
        private readonly IAuthService _authService;
        private readonly ILogger<DangKyController> _logger;

        public DangKyController(
            IDangKyService dangKyService,
            IGoiTapService goiTapService,
            ILopHocService lopHocService,
            INguoiDungService nguoiDungService,
            IAuthService authService,
            ILogger<DangKyController> logger)
        {
            _dangKyService = dangKyService;
            _goiTapService = goiTapService;
            _lopHocService = lopHocService;
            _nguoiDungService = nguoiDungService;
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var registrations = await _dangKyService.GetAllAsync();
                return View(registrations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting registrations");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách đăng ký.";
                return View(new List<DangKy>());
            }
        }

        // MyRegistrations action đã được chuyển sang MemberController để tránh trùng lặp

        public async Task<IActionResult> RegisterPackage()
        {
            await LoadPackagesSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPackage(int packageId, int duration)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user?.NguoiDungId == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var result = await _dangKyService.RegisterPackageAsync(user.NguoiDungId.Value, packageId, duration);
                if (result)
                {
                    TempData["SuccessMessage"] = "Đăng ký gói tập thành công!";
                    return RedirectToAction("MyRegistrations", "Member");
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể đăng ký gói tập. Bạn có thể đã có gói tập đang hoạt động.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering package");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng ký gói tập.";
            }

            await LoadPackagesSelectList();
            return View();
        }

        public async Task<IActionResult> RegisterClass()
        {
            await LoadClassesSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterClass(int classId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user?.NguoiDungId == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var result = await _dangKyService.RegisterClassAsync(user.NguoiDungId.Value, classId, startDate, endDate);
                if (result)
                {
                    TempData["SuccessMessage"] = "Đăng ký lớp học thành công!";
                    return RedirectToAction("MyRegistrations", "Member");
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể đăng ký lớp học. Bạn có thể đã đăng ký lớp này rồi.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering class");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng ký lớp học.";
            }

            await LoadClassesSelectList();
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await LoadSelectLists();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(DangKy dangKy)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _dangKyService.CreateAsync(dangKy);
                    TempData["SuccessMessage"] = "Tạo đăng ký thành công!";
                    return RedirectToAction(nameof(Index));
                }
                await LoadSelectLists();
                return View(dangKy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating registration");
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo đăng ký.");
                await LoadSelectLists();
                return View(dangKy);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var dangKy = await _dangKyService.GetByIdAsync(id);
                if (dangKy == null)
                {
                    return NotFound();
                }
                return View(dangKy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting registration details for ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin đăng ký.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Extend(int id, int additionalMonths)
        {
            try
            {
                var result = await _dangKyService.ExtendRegistrationAsync(id, additionalMonths);
                if (result)
                {
                    return Json(new { success = true, message = "Gia hạn đăng ký thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể gia hạn đăng ký." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while extending registration");
                return Json(new { success = false, message = "Có lỗi xảy ra khi gia hạn đăng ký." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cancel(int id, string reason)
        {
            try
            {
                var result = await _dangKyService.CancelRegistrationAsync(id, reason);
                if (result)
                {
                    return Json(new { success = true, message = "Hủy đăng ký thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể hủy đăng ký." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while canceling registration");
                return Json(new { success = false, message = "Có lỗi xảy ra khi hủy đăng ký." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CalculateFee(int packageId, int duration, int? promotionId = null)
        {
            try
            {
                var fee = await _dangKyService.CalculateRegistrationFeeAsync(packageId, duration, promotionId);
                return Json(new { 
                    success = true, 
                    fee = fee,
                    formattedFee = fee.ToString("N0") + " VNĐ"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while calculating registration fee");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tính phí đăng ký." });
            }
        }

        public async Task<IActionResult> ActiveRegistrations()
        {
            try
            {
                var registrations = await _dangKyService.GetActiveRegistrationsAsync();
                return View(registrations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting active registrations");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách đăng ký đang hoạt động.";
                return View(new List<DangKy>());
            }
        }

        public async Task<IActionResult> ExpiredRegistrations()
        {
            try
            {
                var registrations = await _dangKyService.GetExpiredRegistrationsAsync();
                return View(registrations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting expired registrations");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách đăng ký đã hết hạn.";
                return View(new List<DangKy>());
            }
        }

        private async Task LoadSelectLists()
        {
            try
            {
                var packages = await _goiTapService.GetAllAsync();
                var classes = await _lopHocService.GetActiveClassesAsync();
                var members = await _nguoiDungService.GetMembersAsync();

                ViewBag.Packages = new SelectList(packages, "GoiTapId", "TenGoi");
                ViewBag.Classes = new SelectList(classes, "LopHocId", "TenLop");
                ViewBag.Members = new SelectList(members, "NguoiDungId", "Ho");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading select lists");
                ViewBag.Packages = new SelectList(new List<GoiTap>(), "GoiTapId", "TenGoi");
                ViewBag.Classes = new SelectList(new List<LopHoc>(), "LopHocId", "TenLop");
                ViewBag.Members = new SelectList(new List<NguoiDung>(), "NguoiDungId", "Ho");
            }
        }

        private async Task LoadPackagesSelectList()
        {
            try
            {
                var packages = await _goiTapService.GetAllAsync();
                ViewBag.Packages = new SelectList(packages, "GoiTapId", "TenGoi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading packages select list");
                ViewBag.Packages = new SelectList(new List<GoiTap>(), "GoiTapId", "TenGoi");
            }
        }

        private async Task LoadClassesSelectList()
        {
            try
            {
                var classes = await _lopHocService.GetActiveClassesAsync();
                ViewBag.Classes = new SelectList(classes, "LopHocId", "TenLop");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading classes select list");
                ViewBag.Classes = new SelectList(new List<LopHoc>(), "LopHocId", "TenLop");
            }
        }
    }
}
