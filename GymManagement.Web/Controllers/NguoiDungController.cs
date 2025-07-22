using GymManagement.Web.Data.Models;
using GymManagement.Web.Models.DTOs;
using GymManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers
{
    [Authorize]
    public class NguoiDungController : Controller
    {
        private readonly INguoiDungService _nguoiDungService;
        private readonly ILogger<NguoiDungController> _logger;

        public NguoiDungController(INguoiDungService nguoiDungService, ILogger<NguoiDungController> logger)
        {
            _nguoiDungService = nguoiDungService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var nguoiDungs = await _nguoiDungService.GetAllAsync();
                return View(nguoiDungs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting users");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách người dùng.";
                return View(new List<NguoiDung>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var nguoiDung = await _nguoiDungService.GetByIdAsync(id);
                if (nguoiDung == null)
                {
                    return NotFound();
                }
                return View(nguoiDung);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user details for ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin người dùng.";
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateNguoiDungDto createDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _nguoiDungService.CreateAsync(createDto);
                    TempData["SuccessMessage"] = "Tạo người dùng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                return View(createDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user");
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo người dùng.");
                return View(createDto);
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var nguoiDung = await _nguoiDungService.GetByIdAsync(id);
                if (nguoiDung == null)
                {
                    return NotFound();
                }
                return View(nguoiDung);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user for edit, ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin người dùng.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, UpdateNguoiDungDto updateDto)
        {
            if (id != updateDto.NguoiDungId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    await _nguoiDungService.UpdateAsync(updateDto);
                    TempData["SuccessMessage"] = "Cập nhật người dùng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user ID: {Id}", id);
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật người dùng.");
                return View(updateDto);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var nguoiDung = await _nguoiDungService.GetByIdAsync(id);
                if (nguoiDung == null)
                {
                    return NotFound();
                }
                return View(nguoiDung);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user for delete, ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin người dùng.";
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
                var result = await _nguoiDungService.DeleteAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Xóa người dùng thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa người dùng.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa người dùng.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMembers()
        {
            try
            {
                var members = await _nguoiDungService.GetMembersAsync();
                return Json(members.Select(m => new { 
                    id = m.NguoiDungId, 
                    text = $"{m.Ho} {m.Ten} - {m.Email}" 
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting members");
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTrainers()
        {
            try
            {
                var trainers = await _nguoiDungService.GetTrainersAsync();
                return Json(trainers.Select(t => new { 
                    id = t.NguoiDungId, 
                    text = $"{t.Ho} {t.Ten} - {t.Email}" 
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting trainers");
                return Json(new List<object>());
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var nguoiDung = await _nguoiDungService.GetByIdAsync(id);
                if (nguoiDung == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng." });
                }

                var updateDto = new UpdateNguoiDungDto
                {
                    NguoiDungId = nguoiDung.NguoiDungId,
                    LoaiNguoiDung = nguoiDung.LoaiNguoiDung,
                    Ho = nguoiDung.Ho,
                    Ten = nguoiDung.Ten,
                    GioiTinh = nguoiDung.GioiTinh,
                    NgaySinh = nguoiDung.NgaySinh,
                    SoDienThoai = nguoiDung.SoDienThoai,
                    Email = nguoiDung.Email,
                    TrangThai = nguoiDung.TrangThai == "ACTIVE" ? "INACTIVE" : "ACTIVE"
                };
                await _nguoiDungService.UpdateAsync(updateDto);

                return Json(new {
                    success = true,
                    message = $"Đã {(updateDto.TrangThai == "ACTIVE" ? "kích hoạt" : "vô hiệu hóa")} người dùng.",
                    newStatus = updateDto.TrangThai
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling user status, ID: {Id}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi thay đổi trạng thái." });
            }
        }
    }
}
