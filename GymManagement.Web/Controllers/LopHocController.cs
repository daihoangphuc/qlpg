using GymManagement.Web.Data.Models;
using GymManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymManagement.Web.Controllers
{
    [Authorize]
    public class LopHocController : Controller
    {
        private readonly ILopHocService _lopHocService;
        private readonly INguoiDungService _nguoiDungService;
        private readonly ILogger<LopHocController> _logger;

        public LopHocController(
            ILopHocService lopHocService, 
            INguoiDungService nguoiDungService,
            ILogger<LopHocController> logger)
        {
            _lopHocService = lopHocService;
            _nguoiDungService = nguoiDungService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var lopHocs = await _lopHocService.GetAllAsync();
                return View(lopHocs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting classes");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách lớp học.";
                return View(new List<LopHoc>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var lopHoc = await _lopHocService.GetByIdAsync(id);
                if (lopHoc == null)
                {
                    return NotFound();
                }
                return View(lopHoc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting class details for ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin lớp học.";
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create()
        {
            await LoadTrainersSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(LopHoc lopHoc)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _lopHocService.CreateAsync(lopHoc);
                    TempData["SuccessMessage"] = "Tạo lớp học thành công!";
                    return RedirectToAction(nameof(Index));
                }
                await LoadTrainersSelectList();
                return View(lopHoc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating class");
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo lớp học.");
                await LoadTrainersSelectList();
                return View(lopHoc);
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var lopHoc = await _lopHocService.GetByIdAsync(id);
                if (lopHoc == null)
                {
                    return NotFound();
                }
                await LoadTrainersSelectList();
                return View(lopHoc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting class for edit, ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin lớp học.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, LopHoc lopHoc)
        {
            if (id != lopHoc.LopHocId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    await _lopHocService.UpdateAsync(lopHoc);
                    TempData["SuccessMessage"] = "Cập nhật lớp học thành công!";
                    return RedirectToAction(nameof(Index));
                }
                await LoadTrainersSelectList();
                return View(lopHoc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating class ID: {Id}", id);
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật lớp học.");
                await LoadTrainersSelectList();
                return View(lopHoc);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var lopHoc = await _lopHocService.GetByIdAsync(id);
                if (lopHoc == null)
                {
                    return NotFound();
                }
                return View(lopHoc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting class for delete, ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin lớp học.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _lopHocService.DeleteAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Xóa lớp học thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa lớp học.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting class ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa lớp học.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveClasses()
        {
            try
            {
                var classes = await _lopHocService.GetActiveClassesAsync();
                return Json(classes.Select(c => new { 
                    id = c.LopHocId, 
                    text = $"{c.TenLop} - {c.GioBatDau:HH:mm}-{c.GioKetThuc:HH:mm}",
                    capacity = c.SucChua,
                    price = c.GiaTuyChinh
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting active classes");
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int classId, DateTime date)
        {
            try
            {
                var isAvailable = await _lopHocService.IsClassAvailableAsync(classId, date);
                var availableSlots = await _lopHocService.GetAvailableSlotsAsync(classId, date);
                
                return Json(new { 
                    available = isAvailable, 
                    slots = availableSlots 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking class availability");
                return Json(new { available = false, slots = 0 });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GenerateSchedule(int classId, DateTime startDate, DateTime endDate)
        {
            try
            {
                await _lopHocService.GenerateScheduleAsync(classId, startDate, endDate);
                return Json(new { success = true, message = "Tạo lịch học thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating schedule");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo lịch học." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSchedule(int classId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var schedule = await _lopHocService.GetClassScheduleAsync(classId, startDate, endDate);
                return Json(schedule.Select(s => new {
                    id = s.LichLopId,
                    date = s.Ngay.ToString("yyyy-MM-dd"),
                    startTime = s.GioBatDau.ToString("HH:mm"),
                    endTime = s.GioKetThuc.ToString("HH:mm"),
                    status = s.TrangThai
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting class schedule");
                return Json(new List<object>());
            }
        }

        public async Task<IActionResult> PublicClasses()
        {
            try
            {
                var classes = await _lopHocService.GetActiveClassesAsync();
                return View(classes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting public classes");
                return View(new List<LopHoc>());
            }
        }

        private async Task LoadTrainersSelectList()
        {
            try
            {
                var trainers = await _nguoiDungService.GetTrainersAsync();
                ViewBag.Trainers = new SelectList(trainers, "NguoiDungId", "Ho", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading trainers select list");
                ViewBag.Trainers = new SelectList(new List<NguoiDung>(), "NguoiDungId", "Ho");
            }
        }
    }
}
