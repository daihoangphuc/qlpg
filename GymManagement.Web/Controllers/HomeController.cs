using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Web.Models;
using GymManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using GymManagement.Web.Data.Models;

namespace GymManagement.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IBaoCaoService _baoCaoService;
    private readonly IGoiTapService _goiTapService;
    private readonly ILopHocService _lopHocService;
    private readonly UserManager<TaiKhoan> _userManager;

    public HomeController(
        ILogger<HomeController> logger,
        IBaoCaoService baoCaoService,
        IGoiTapService goiTapService,
        ILopHocService lopHocService,
        UserManager<TaiKhoan> userManager)
    {
        _logger = logger;
        _baoCaoService = baoCaoService;
        _goiTapService = goiTapService;
        _lopHocService = lopHocService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                // For authenticated users, show dashboard
                var dashboardData = await _baoCaoService.GetDashboardDataAsync();
                return View("Dashboard", dashboardData);
            }
            else
            {
                // For anonymous users, show public landing page
                var packages = await _goiTapService.GetAllAsync();
                var classes = await _lopHocService.GetActiveClassesAsync();

                ViewBag.Packages = packages.Take(3); // Show top 3 packages
                ViewBag.Classes = classes.Take(4);   // Show top 4 classes

                return View("Landing");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading home page");
            return View();
        }
    }

    [Authorize]
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
            return View(classes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading classes");
            return View(new List<LopHocDto>());
        }
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
}
