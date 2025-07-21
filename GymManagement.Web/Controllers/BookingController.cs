using GymManagement.Web.Data.Models;
using GymManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymManagement.Web.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ILopHocService _lopHocService;
        private readonly INguoiDungService _nguoiDungService;
        private readonly UserManager<TaiKhoan> _userManager;
        private readonly ILogger<BookingController> _logger;

        public BookingController(
            IBookingService bookingService,
            ILopHocService lopHocService,
            INguoiDungService nguoiDungService,
            UserManager<TaiKhoan> userManager,
            ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _lopHocService = lopHocService;
            _nguoiDungService = nguoiDungService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var bookings = await _bookingService.GetAllAsync();
                return View(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting bookings");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách đặt lịch.";
                return View(new List<Booking>());
            }
        }

        public async Task<IActionResult> MyBookings()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.NguoiDungId == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var bookings = await _bookingService.GetByMemberIdAsync(user.NguoiDungId.Value);
                return View(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user bookings");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách đặt lịch của bạn.";
                return View(new List<Booking>());
            }
        }

        public async Task<IActionResult> Create()
        {
            await LoadSelectLists();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // If no member is selected, use current user
                    if (booking.ThanhVienId == null)
                    {
                        var user = await _userManager.GetUserAsync(User);
                        if (user?.NguoiDungId != null)
                        {
                            booking.ThanhVienId = user.NguoiDungId.Value;
                        }
                    }

                    await _bookingService.CreateAsync(booking);
                    TempData["SuccessMessage"] = "Đặt lịch thành công!";
                    return RedirectToAction(nameof(MyBookings));
                }
                await LoadSelectLists();
                return View(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating booking");
                ModelState.AddModelError("", "Có lỗi xảy ra khi đặt lịch.");
                await LoadSelectLists();
                return View(booking);
            }
        }

        [HttpPost]
        public async Task<IActionResult> BookClass(int classId, DateTime date)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.NguoiDungId == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để đặt lịch." });
                }

                var result = await _bookingService.BookClassAsync(user.NguoiDungId.Value, classId, date);
                if (result)
                {
                    return Json(new { success = true, message = "Đặt lịch thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể đặt lịch. Lớp có thể đã đầy hoặc bạn đã đặt lịch rồi." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while booking class");
                return Json(new { success = false, message = "Có lỗi xảy ra khi đặt lịch." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BookSchedule(int scheduleId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.NguoiDungId == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để đặt lịch." });
                }

                var result = await _bookingService.BookScheduleAsync(user.NguoiDungId.Value, scheduleId);
                if (result)
                {
                    return Json(new { success = true, message = "Đặt lịch thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể đặt lịch. Lớp có thể đã đầy hoặc bạn đã đặt lịch rồi." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while booking schedule");
                return Json(new { success = false, message = "Có lỗi xảy ra khi đặt lịch." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var result = await _bookingService.CancelBookingAsync(id);
                if (result)
                {
                    return Json(new { success = true, message = "Hủy đặt lịch thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể hủy đặt lịch." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while canceling booking");
                return Json(new { success = false, message = "Có lỗi xảy ra khi hủy đặt lịch." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int classId, DateTime date)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.NguoiDungId == null)
                {
                    return Json(new { canBook = false, message = "Vui lòng đăng nhập." });
                }

                var canBook = await _bookingService.CanBookAsync(user.NguoiDungId.Value, classId, date);
                var availableSlots = await _bookingService.GetAvailableSlotsAsync(classId, date);

                return Json(new { 
                    canBook = canBook, 
                    availableSlots = availableSlots,
                    message = canBook ? "Có thể đặt lịch" : "Không thể đặt lịch"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking booking availability");
                return Json(new { canBook = false, message = "Có lỗi xảy ra." });
            }
        }

        public async Task<IActionResult> TodayBookings()
        {
            try
            {
                var bookings = await _bookingService.GetTodayBookingsAsync();
                return View(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting today's bookings");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách đặt lịch hôm nay.";
                return View(new List<Booking>());
            }
        }

        public async Task<IActionResult> Calendar()
        {
            try
            {
                var classes = await _lopHocService.GetActiveClassesAsync();
                ViewBag.Classes = classes;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading calendar");
                ViewBag.Classes = new List<LopHoc>();
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCalendarEvents(DateTime start, DateTime end)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.NguoiDungId == null)
                {
                    return Json(new List<object>());
                }

                var bookings = await _bookingService.GetByMemberIdAsync(user.NguoiDungId.Value);
                var events = bookings
                    .Where(b => b.Ngay >= start.Date && b.Ngay <= end.Date && b.TrangThai == "BOOKED")
                    .Select(b => new {
                        id = b.BookingId,
                        title = b.LopHoc?.TenLop ?? "Lớp học",
                        start = b.Ngay.ToString("yyyy-MM-dd") + "T" + (b.LichLop?.GioBatDau.ToString("HH:mm") ?? "00:00"),
                        end = b.Ngay.ToString("yyyy-MM-dd") + "T" + (b.LichLop?.GioKetThuc.ToString("HH:mm") ?? "01:00"),
                        color = b.TrangThai == "BOOKED" ? "#3b82f6" : "#ef4444"
                    });

                return Json(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting calendar events");
                return Json(new List<object>());
            }
        }

        private async Task LoadSelectLists()
        {
            try
            {
                var classes = await _lopHocService.GetActiveClassesAsync();
                ViewBag.Classes = new SelectList(classes, "LopHocId", "TenLop");

                // Only load members for admin/staff
                if (User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Staff"))
                {
                    var members = await _nguoiDungService.GetMembersAsync();
                    ViewBag.Members = new SelectList(members, "NguoiDungId", "Ho");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading select lists");
                ViewBag.Classes = new SelectList(new List<LopHoc>(), "LopHocId", "TenLop");
                ViewBag.Members = new SelectList(new List<NguoiDung>(), "NguoiDungId", "Ho");
            }
        }
    }
}
