using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GymManagement.Web.Services;
using GymManagement.Web.Models.DTOs;

namespace GymManagement.Web.Controllers
{
    [Authorize]
    public class HuanLuyenVienController : Controller
    {
        private readonly INguoiDungService _nguoiDungService;
        private readonly ILogger<HuanLuyenVienController> _logger;

        public HuanLuyenVienController(INguoiDungService nguoiDungService, ILogger<HuanLuyenVienController> logger)
        {
            _nguoiDungService = nguoiDungService;
            _logger = logger;
        }

        // GET: HuanLuyenVien
        public async Task<IActionResult> Index()
        {
            try
            {
                var huanLuyenViens = await _nguoiDungService.GetByLoaiNguoiDungAsync("HLV");
                return View(huanLuyenViens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting trainers");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách huấn luyện viên.";
                return View(new List<NguoiDungDto>());
            }
        }

        // GET: HuanLuyenVien/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var huanLuyenVien = await _nguoiDungService.GetByIdAsync(id);
                if (huanLuyenVien == null || huanLuyenVien.LoaiNguoiDung != "HLV")
                {
                    return NotFound();
                }
                return View(huanLuyenVien);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting trainer details for ID: {Id}", id);
                return NotFound();
            }
        }

        // GET: HuanLuyenVien/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var createDto = new CreateNguoiDungDto
            {
                LoaiNguoiDung = "HLV"
            };
            return View(createDto);
        }

        // POST: HuanLuyenVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateNguoiDungDto createDto)
        {
            try
            {
                // Ensure it's a trainer
                createDto.LoaiNguoiDung = "HLV";

                if (ModelState.IsValid)
                {
                    await _nguoiDungService.CreateAsync(createDto);
                    TempData["SuccessMessage"] = "Tạo huấn luyện viên thành công!";
                    return RedirectToAction(nameof(Index));
                }
                return View(createDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating trainer");
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo huấn luyện viên.");
                return View(createDto);
            }
        }

        // GET: HuanLuyenVien/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var huanLuyenVien = await _nguoiDungService.GetByIdAsync(id);
                if (huanLuyenVien == null || huanLuyenVien.LoaiNguoiDung != "HLV")
                {
                    return NotFound();
                }

                var updateDto = new UpdateNguoiDungDto
                {
                    NguoiDungId = huanLuyenVien.NguoiDungId,
                    LoaiNguoiDung = huanLuyenVien.LoaiNguoiDung,
                    Ho = huanLuyenVien.Ho,
                    Ten = huanLuyenVien.Ten,
                    GioiTinh = huanLuyenVien.GioiTinh,
                    NgaySinh = huanLuyenVien.NgaySinh,
                    SoDienThoai = huanLuyenVien.SoDienThoai,
                    Email = huanLuyenVien.Email,
                    TrangThai = huanLuyenVien.TrangThai,
                    NgayThamGia = huanLuyenVien.NgayThamGia,
                    NgayTao = huanLuyenVien.NgayTao
                };

                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting trainer for edit, ID: {Id}", id);
                return NotFound();
            }
        }

        // POST: HuanLuyenVien/Edit/5
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
                // Ensure it remains a trainer
                updateDto.LoaiNguoiDung = "HLV";

                if (ModelState.IsValid)
                {
                    await _nguoiDungService.UpdateAsync(updateDto);
                    TempData["SuccessMessage"] = "Cập nhật huấn luyện viên thành công!";
                    return RedirectToAction(nameof(Index));
                }
                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating trainer ID: {Id}", id);
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật huấn luyện viên.");
                return View(updateDto);
            }
        }

        // GET: HuanLuyenVien/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var huanLuyenVien = await _nguoiDungService.GetByIdAsync(id);
                if (huanLuyenVien == null || huanLuyenVien.LoaiNguoiDung != "HLV")
                {
                    return NotFound();
                }
                return View(huanLuyenVien);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting trainer for delete, ID: {Id}", id);
                return NotFound();
            }
        }

        // POST: HuanLuyenVien/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _nguoiDungService.DeleteAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Xóa huấn luyện viên thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa huấn luyện viên.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting trainer ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa huấn luyện viên.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: HuanLuyenVien/ToggleStatus/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var huanLuyenVien = await _nguoiDungService.GetByIdAsync(id);
                if (huanLuyenVien == null || huanLuyenVien.LoaiNguoiDung != "HLV")
                {
                    return Json(new { success = false, message = "Không tìm thấy huấn luyện viên." });
                }

                var updateDto = new UpdateNguoiDungDto
                {
                    NguoiDungId = huanLuyenVien.NguoiDungId,
                    LoaiNguoiDung = huanLuyenVien.LoaiNguoiDung,
                    Ho = huanLuyenVien.Ho,
                    Ten = huanLuyenVien.Ten,
                    GioiTinh = huanLuyenVien.GioiTinh,
                    NgaySinh = huanLuyenVien.NgaySinh,
                    SoDienThoai = huanLuyenVien.SoDienThoai,
                    Email = huanLuyenVien.Email,
                    TrangThai = huanLuyenVien.TrangThai == "ACTIVE" ? "INACTIVE" : "ACTIVE",
                    NgayThamGia = huanLuyenVien.NgayThamGia,
                    NgayTao = huanLuyenVien.NgayTao
                };

                await _nguoiDungService.UpdateAsync(updateDto);

                return Json(new { 
                    success = true, 
                    message = $"Đã {(updateDto.TrangThai == "ACTIVE" ? "kích hoạt" : "vô hiệu hóa")} huấn luyện viên.",
                    newStatus = updateDto.TrangThai
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling trainer status ID: {Id}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi thay đổi trạng thái huấn luyện viên." });
            }
        }
    }
}
