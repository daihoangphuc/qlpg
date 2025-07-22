using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Web.Models;
using GymManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using GymManagement.Web.Data.Models;
using GymManagement.Web.Data;
using GymManagement.Web.Models.DTOs;

namespace GymManagement.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IBaoCaoService _baoCaoService;
    private readonly IGoiTapService _goiTapService;
    private readonly ILopHocService _lopHocService;
    private readonly IAuthService _authService;
    private readonly GymDbContext _context;

    public HomeController(
        ILogger<HomeController> logger,
        IBaoCaoService baoCaoService,
        IGoiTapService goiTapService,
        ILopHocService lopHocService,
        IAuthService authService,
        GymDbContext context)
    {
        _logger = logger;
        _baoCaoService = baoCaoService;
        _goiTapService = goiTapService;
        _lopHocService = lopHocService;
        _authService = authService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            // Check user role to determine which view to show
            if (User.IsInRole("Admin"))
            {
                // For admin users, redirect to dashboard
                return RedirectToAction("Dashboard");
            }
        }

        // Load packages separately
        try
        {
            var packages = await _goiTapService.GetAllAsync();
            ViewBag.Packages = packages; // Show all packages
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading packages");
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

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            var dashboardData = await _baoCaoService.GetDashboardDataAsync();
            return View(dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading dashboard");
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dashboard.";
            return View(new { });
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
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet]
    public async Task<IActionResult> GetRealtimeStats()
    {
        try
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var stats = await _baoCaoService.GetRealtimeStatsAsync();
                return Json(stats);
            }
            return Json(new { });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting realtime stats");
            return Json(new { });
        }
    }

    [HttpGet]
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
}
