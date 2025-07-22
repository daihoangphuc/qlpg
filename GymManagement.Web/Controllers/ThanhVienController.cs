using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GymManagement.Web.Services;
using GymManagement.Web.Models.DTOs;

namespace GymManagement.Web.Controllers
{
    [Authorize]
    public class ThanhVienController : Controller
    {
        private readonly INguoiDungService _nguoiDungService;
        private readonly ILogger<ThanhVienController> _logger;

        public ThanhVienController(INguoiDungService nguoiDungService, ILogger<ThanhVienController> logger)
        {
            _nguoiDungService = nguoiDungService;
            _logger = logger;
        }

        // GET: ThanhVien
        public async Task<IActionResult> Index()
        {
            try
            {
                var thanhViens = await _nguoiDungService.GetByLoaiNguoiDungAsync("THANHVIEN");
                return View(thanhViens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting members");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách thành viên.";
                return View(new List<NguoiDungDto>());
            }
        }

        // GET: ThanhVien/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var thanhVien = await _nguoiDungService.GetByIdAsync(id);
                if (thanhVien == null || thanhVien.LoaiNguoiDung != "THANHVIEN")
                {
                    return NotFound();
                }
                return View(thanhVien);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting member details for ID: {Id}", id);
                return NotFound();
            }
        }

        // GET: ThanhVien/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var createDto = new CreateNguoiDungDto
            {
                LoaiNguoiDung = "THANHVIEN"
            };
            return View(createDto);
        }

        // POST: ThanhVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateNguoiDungDto createDto)
        {
            try
            {
                // Ensure it's a member
                createDto.LoaiNguoiDung = "THANHVIEN";

                if (ModelState.IsValid)
                {
                    await _nguoiDungService.CreateAsync(createDto);
                    TempData["SuccessMessage"] = "Tạo thành viên thành công!";
                    return RedirectToAction(nameof(Index));
                }
                return View(createDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating member");
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo thành viên.");
                return View(createDto);
            }
        }

        // GET: ThanhVien/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var thanhVien = await _nguoiDungService.GetByIdAsync(id);
                if (thanhVien == null || thanhVien.LoaiNguoiDung != "THANHVIEN")
                {
                    return NotFound();
                }

                var updateDto = new UpdateNguoiDungDto
                {
                    NguoiDungId = thanhVien.NguoiDungId,
                    LoaiNguoiDung = thanhVien.LoaiNguoiDung,
                    Ho = thanhVien.Ho,
                    Ten = thanhVien.Ten,
                    GioiTinh = thanhVien.GioiTinh,
                    NgaySinh = thanhVien.NgaySinh,
                    SoDienThoai = thanhVien.SoDienThoai,
                    Email = thanhVien.Email,
                    TrangThai = thanhVien.TrangThai,
                    NgayThamGia = thanhVien.NgayThamGia,
                    NgayTao = thanhVien.NgayTao
                };

                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting member for edit, ID: {Id}", id);
                return NotFound();
            }
        }

        // POST: ThanhVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, UpdateNguoiDungDto updateDto)
        {
            if (id != updateDto.NguoiDungId)
            {
                return NotFound();
            }

            try
            {
                // Ensure it remains a member
                updateDto.LoaiNguoiDung = "THANHVIEN";

                if (ModelState.IsValid)
                {
                    await _nguoiDungService.UpdateAsync(updateDto);
                    TempData["SuccessMessage"] = "Cập nhật thành viên thành công!";
                    return RedirectToAction(nameof(Index));
                }
                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating member ID: {Id}", id);
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật thành viên.");
                return View(updateDto);
            }
        }

        // GET: ThanhVien/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var thanhVien = await _nguoiDungService.GetByIdAsync(id);
                if (thanhVien == null || thanhVien.LoaiNguoiDung != "THANHVIEN")
                {
                    return NotFound();
                }
                return View(thanhVien);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting member for delete, ID: {Id}", id);
                return NotFound();
            }
        }

        // POST: ThanhVien/Delete/5
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
                    TempData["SuccessMessage"] = "Xóa thành viên thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa thành viên.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting member ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa thành viên.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ThanhVien/ToggleStatus/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var thanhVien = await _nguoiDungService.GetByIdAsync(id);
                if (thanhVien == null || thanhVien.LoaiNguoiDung != "THANHVIEN")
                {
                    return Json(new { success = false, message = "Không tìm thấy thành viên." });
                }

                var updateDto = new UpdateNguoiDungDto
                {
                    NguoiDungId = thanhVien.NguoiDungId,
                    LoaiNguoiDung = thanhVien.LoaiNguoiDung,
                    Ho = thanhVien.Ho,
                    Ten = thanhVien.Ten,
                    GioiTinh = thanhVien.GioiTinh,
                    NgaySinh = thanhVien.NgaySinh,
                    SoDienThoai = thanhVien.SoDienThoai,
                    Email = thanhVien.Email,
                    TrangThai = thanhVien.TrangThai == "ACTIVE" ? "INACTIVE" : "ACTIVE",
                    NgayThamGia = thanhVien.NgayThamGia,
                    NgayTao = thanhVien.NgayTao
                };

                await _nguoiDungService.UpdateAsync(updateDto);

                return Json(new { 
                    success = true, 
                    message = $"Đã {(updateDto.TrangThai == "ACTIVE" ? "kích hoạt" : "vô hiệu hóa")} thành viên.",
                    newStatus = updateDto.TrangThai
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling member status ID: {Id}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi thay đổi trạng thái thành viên." });
            }
        }
    }
}
