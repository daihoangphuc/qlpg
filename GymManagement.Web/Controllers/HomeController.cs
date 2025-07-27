using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Web.Models;
using GymManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using GymManagement.Web.Data.Models;
using GymManagement.Web.Data;
using GymManagement.Web.Models.DTOs;
using GymManagement.Web.Models.ViewModels;

namespace GymManagement.Web.Controllers;

public class HomeController : BaseController
{
    private readonly IBaoCaoService _baoCaoService;
    private readonly IGoiTapService _goiTapService;
    private readonly ILopHocService _lopHocService;
    private readonly IAuthService _authService;
    private readonly GymDbContext _context;
    private readonly IDangKyService _dangKyService;
    private readonly IDiemDanhService _diemDanhService;

    public HomeController(
        IUserSessionService userSessionService,
        ILogger<HomeController> logger,
        IBaoCaoService baoCaoService,
        IGoiTapService goiTapService,
        ILopHocService lopHocService,
        IAuthService authService,
        GymDbContext context,
        IDangKyService dangKyService,
        IDiemDanhService diemDanhService) : base(userSessionService, logger)
    {
        _baoCaoService = baoCaoService;
        _goiTapService = goiTapService;
        _lopHocService = lopHocService;
        _authService = authService;
        _context = context;
        _dangKyService = dangKyService;
        _diemDanhService = diemDanhService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            if (_userSessionService.IsUserAuthenticated())
            {
                // Check user role to determine which view to show
                if (IsInRoleSafe("Admin"))
                {
                    // For admin users, redirect to dashboard
                    return RedirectToAction("Dashboard");
                }
                // Members will see the home page with member-specific content
            }

        // Load popular packages separately
        try
        {
            var popularPackages = await _goiTapService.GetPopularPackagesAsync();

            // If user is authenticated and is a member, filter out registered packages
            if (_userSessionService.IsUserAuthenticated() && IsInRoleSafe("Member"))
            {
                var nguoiDungId = GetCurrentNguoiDungIdSafe();
                if (nguoiDungId.HasValue && nguoiDungId.Value > 0)
                {
                    // Get user's active registrations
                    var userRegistrations = await _dangKyService.GetActiveRegistrationsByMemberIdAsync(nguoiDungId.Value);
                    var registeredPackageIds = userRegistrations
                        .Where(r => r.GoiTapId.HasValue)
                        .Select(r => r.GoiTapId!.Value)
                        .ToHashSet();

                    // Filter out already registered packages
                    popularPackages = popularPackages.Where(p => !registeredPackageIds.Contains(p.GoiTapId)).ToList();
                }
            }

            ViewBag.Packages = popularPackages.Take(6).ToList(); // Show top 6 available packages
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading popular packages");
            ViewBag.Packages = new List<GoiTapDto>();
        }

        // Load classes separately
        try
        {
            var classes = await _lopHocService.GetActiveClassesAsync();
            ViewBag.Classes = classes.Take(4);   // Show top 4 classes
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading classes");
            ViewBag.Classes = new List<LopHocDto>();
        }

            return View(); // This will return Index.cshtml (the public home page)
        }
        catch (Exception ex)
        {
            return HandleError(ex, "Có lỗi xảy ra khi tải trang chủ.");
        }
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            // Verify user has admin role using safe method
            if (!IsInRoleSafe("Admin"))
            {
                return HandleUnauthorized();
            }

            var currentUser = await GetCurrentUserSafeAsync();
            if (currentUser == null)
            {
                return HandleUserNotFound("Dashboard");
            }

            LogUserAction("AccessDashboard");

            var dashboardData = await _baoCaoService.GetDashboardDataAsync();
            return View(dashboardData);
        }
        catch (Exception ex)
        {
            return HandleError(ex, "Có lỗi xảy ra khi tải dashboard.");
        }
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    public async Task<IActionResult> Packages()
    {
        try
        {
            var packages = await _goiTapService.GetAllAsync();
            return View(packages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading packages");
            return View(new List<GoiTapDto>());
        }
    }

    public async Task<IActionResult> Classes()
    {
        try
        {
            var classes = await _lopHocService.GetActiveClassesAsync();
            var classDtos = classes.Select(c => new LopHocDto
            {
                LopHocId = c.LopHocId,
                TenLop = c.TenLop,
                MoTa = c.MoTa,
                SucChuaToiDa = c.SucChua,
                ThoiLuongPhut = c.ThoiLuong,
                MucDo = GetMucDoFromPrice(c.GiaTuyChinh), // Determine level based on price
                TrangThai = c.TrangThai == "OPEN" ? "ACTIVE" : c.TrangThai,
                NgayTao = DateTime.Now,
                TrainerName = c.Hlv != null ? $"{c.Hlv.Ho} {c.Hlv.Ten}".Trim() : "Chưa phân công",
                RegisteredCount = c.DangKys?.Count(d => d.TrangThai == "ACTIVE") ?? 0
            }).ToList();

            return View(classDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading classes");
            return View(new List<LopHocDto>());
        }
    }

    private string GetMucDoFromPrice(decimal? price)
    {
        if (!price.HasValue) return "Cơ bản";

        return price.Value switch
        {
            <= 200000 => "Cơ bản",
            <= 250000 => "Trung bình",
            _ => "Nâng cao"
        };
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(string? message = null)
    {
        ViewBag.ErrorMessage = message ?? TempData["ErrorMessage"];

        // Show detailed error info only in development
        var environment = HttpContext.RequestServices.GetService<IWebHostEnvironment>();
        if (environment?.IsDevelopment() == true)
        {
            ViewBag.ShowDetails = true;
            // You can add more debug info here if needed
        }

        return View();
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
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
            return Json(new { });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SeedData()
    {
        try
        {
            await DbInitializer.InitializeAsync(_context, _authService);
            return Json(new { success = true, message = "Dữ liệu mẫu đã được tạo thành công!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding data");
            return Json(new { success = false, message = ex.Message });
        }
    }

    // Member Dashboard - accessible for all authenticated users
    [Authorize]
    public async Task<IActionResult> MemberDashboard()
    {
        try
        {
            // Get current user info
            var userIdClaim = User.FindFirst("NguoiDungId")?.Value;
            var taiKhoanId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                // Try to refresh user claims if NguoiDungId is missing
                if (!string.IsNullOrEmpty(taiKhoanId))
                {
                    _logger.LogWarning("NguoiDungId claim missing for user {TaiKhoanId}, attempting to refresh claims", taiKhoanId);

                    // Redirect to a refresh endpoint or show a message
                    TempData["InfoMessage"] = "Đang cập nhật thông tin người dùng, vui lòng thử lại.";
                    return RedirectToAction("RefreshUserClaims");
                }

                TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại.";
                return RedirectToAction("Logout", "Auth");
            }

            var memberId = int.Parse(userIdClaim);

            // Get member info and stats
            ViewBag.MemberId = memberId;
            ViewBag.UserName = User.Identity?.Name;
            ViewBag.TaiKhoanId = taiKhoanId;

            // Load dashboard data
            var registrations = await _dangKyService.GetByMemberIdAsync(memberId);
            var activeRegistrations = registrations.Where(d => d.TrangThai == "ACTIVE" && d.NgayKetThuc >= DateOnly.FromDateTime(DateTime.Today));

            // Calculate stats
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var attendanceCount = await _diemDanhService.GetMemberAttendanceCountAsync(memberId,
                new DateTime(currentYear, currentMonth, 1),
                new DateTime(currentYear, currentMonth, DateTime.DaysInMonth(currentYear, currentMonth)));

            // Create dashboard model
            var dashboardModel = new MemberDashboardViewModel
            {
                TotalActiveRegistrations = activeRegistrations.Count(),
                MonthlyAttendanceCount = attendanceCount,
                TotalClassesJoined = activeRegistrations.Count(r => r.LopHocId != null),
                CurrentPackage = activeRegistrations.FirstOrDefault(r => r.GoiTapId != null)?.GoiTap?.TenGoi ?? "Chưa có",
                RecentRegistrations = registrations.Take(5).ToList()
            };

            return View(dashboardModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading member dashboard");
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dashboard.";
            return RedirectToAction("Index");
        }
    }

    // Refresh user claims endpoint
    [Authorize]
    public async Task<IActionResult> RefreshUserClaims()
    {
        try
        {
            var taiKhoanId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(taiKhoanId))
            {
                TempData["ErrorMessage"] = "Không thể xác định người dùng.";
                return RedirectToAction("Logout", "Auth");
            }

            // Get user from database and recreate claims
            var user = await _authService.GetUserByIdAsync(taiKhoanId);
            if (user != null)
            {
                var principal = await _authService.CreateClaimsPrincipalAsync(user);
                await HttpContext.SignInAsync("Cookies", principal);

                TempData["SuccessMessage"] = "Thông tin người dùng đã được cập nhật.";
                return RedirectToAction("MemberDashboard");
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin tài khoản.";
                return RedirectToAction("Logout", "Auth");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing user claims");
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật thông tin.";
            return RedirectToAction("Index");
        }
    }

    // Debug action to check user claims - ADMIN ONLY for security
    [Authorize(Roles = "Admin")]
    public IActionResult DebugClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        var roles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
        var isInMemberRole = User.IsInRole("Member");

        ViewBag.Claims = claims;
        ViewBag.Roles = roles;
        ViewBag.IsInMemberRole = isInMemberRole;
        ViewBag.IsAuthenticated = User.Identity?.IsAuthenticated;
        ViewBag.UserName = User.Identity?.Name;

        return Json(new {
            claims = claims,
            roles = roles,
            isInMemberRole = isInMemberRole,
            isAuthenticated = User.Identity?.IsAuthenticated,
            userName = User.Identity?.Name
        });
    }


}
