using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Web.Services;
using GymManagement.Web.Models.DTOs;

namespace GymManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly INguoiDungService _nguoiDungService;
        private readonly ILogger<UserController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserController(INguoiDungService nguoiDungService, ILogger<UserController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _nguoiDungService = nguoiDungService;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: User
        public async Task<IActionResult> Index(string? loaiNguoiDung = null)
        {
            try
            {
                IEnumerable<NguoiDungDto> nguoiDungs;
                
                if (!string.IsNullOrEmpty(loaiNguoiDung))
                {
                    nguoiDungs = await _nguoiDungService.GetByLoaiNguoiDungAsync(loaiNguoiDung);
                }
                else
                {
                    nguoiDungs = await _nguoiDungService.GetAllAsync();
                }

                ViewBag.LoaiNguoiDung = loaiNguoiDung;
                return View(nguoiDungs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting users");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách người dùng.";
                return View(new List<NguoiDungDto>());
            }
        }

        // GET: User/Members - Shortcut for THANHVIEN
        public async Task<IActionResult> Members()
        {
            return await Index("THANHVIEN");
        }

        // GET: User/Trainers - Shortcut for HLV  
        public async Task<IActionResult> Trainers()
        {
            return await Index("HLV");
        }

        // GET: User/Staff - Shortcut for NHANVIEN
        public async Task<IActionResult> Staff()
        {
            return await Index("NHANVIEN");
        }

        // GET: User/Guests - Shortcut for VANGLAI
        public async Task<IActionResult> Guests()
        {
            return await Index("VANGLAI");
        }

        // GET: User/Details/5
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

        // GET: User/Create
        public IActionResult Create()
        {
            return View(new CreateNguoiDungDto());
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNguoiDungDto createDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var nguoiDung = await _nguoiDungService.CreateAsync(createDto);
                    TempData["SuccessMessage"] = "Tạo người dùng thành công!";
                    return RedirectToAction(nameof(Details), new { id = nguoiDung.NguoiDungId });
                }

                return View(createDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user");
                ModelState.AddModelError("", ex.Message);
                return View(createDto);
            }
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var nguoiDung = await _nguoiDungService.GetByIdAsync(id);
                if (nguoiDung == null)
                {
                    return NotFound();
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
                    TrangThai = nguoiDung.TrangThai
                };

                ViewBag.CurrentAvatar = nguoiDung.AnhDaiDien;
                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading edit user page for ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải trang chỉnh sửa người dùng.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateNguoiDungDto updateDto, IFormFile? AvatarFile)
        {
            if (id != updateDto.NguoiDungId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Xử lý upload avatar nếu có
                    if (AvatarFile != null && AvatarFile.Length > 0)
                    {
                        var avatarPath = await ProcessAvatarUpload(AvatarFile, updateDto.NguoiDungId);
                        if (!string.IsNullOrEmpty(avatarPath))
                        {
                            // Cập nhật avatar path vào DTO
                            var currentUser = await _nguoiDungService.GetByIdAsync(updateDto.NguoiDungId);
                            if (currentUser != null)
                            {
                                currentUser.AnhDaiDien = avatarPath;
                                await _nguoiDungService.UpdateAsync(currentUser);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "Có lỗi xảy ra khi tải lên ảnh đại diện.");
                            var currentUser = await _nguoiDungService.GetByIdAsync(id);
                            ViewBag.CurrentAvatar = currentUser?.AnhDaiDien;
                            return View(updateDto);
                        }
                    }

                    var nguoiDung = await _nguoiDungService.UpdateAsync(updateDto);
                    TempData["SuccessMessage"] = "Cập nhật người dùng thành công!";
                    return RedirectToAction(nameof(Details), new { id = nguoiDung.NguoiDungId });
                }

                // Reload current avatar for view
                var currentUserForView = await _nguoiDungService.GetByIdAsync(id);
                ViewBag.CurrentAvatar = currentUserForView?.AnhDaiDien;
                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user ID: {Id}", id);
                ModelState.AddModelError("", ex.Message);

                // Reload current avatar for view
                var currentUser = await _nguoiDungService.GetByIdAsync(id);
                ViewBag.CurrentAvatar = currentUser?.AnhDaiDien;
                return View(updateDto);
            }
        }

        // POST: User/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _nguoiDungService.DeleteAsync(id);
                if (result)
                {
                    return Json(new { success = true, message = "Xóa người dùng thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể xóa người dùng." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user ID: {Id}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Xử lý upload avatar (tương tự ProfileController)
        private async Task<string?> ProcessAvatarUpload(IFormFile avatarFile, int userId)
        {
            try
            {
                // Kiểm tra file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    _logger.LogWarning("Invalid file extension for avatar upload: {Extension}", fileExtension);
                    return null;
                }

                // Kiểm tra file size (max 5MB)
                if (avatarFile.Length > 5 * 1024 * 1024)
                {
                    _logger.LogWarning("Avatar file too large: {Size} bytes", avatarFile.Length);
                    return null;
                }

                // Tạo thư mục uploads nếu chưa có
                var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Tạo tên file unique
                var fileName = $"avatar_{userId}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Xóa avatar cũ nếu có
                await DeleteOldAvatar(userId);

                // Lưu file mới
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }

                // Trả về relative path để lưu vào database
                return $"/uploads/avatars/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing avatar upload for user {UserId}", userId);
                return null;
            }
        }

        // Xóa avatar cũ
        private async Task DeleteOldAvatar(int userId)
        {
            try
            {
                var user = await _nguoiDungService.GetByIdAsync(userId);
                if (user?.AnhDaiDien != null && user.AnhDaiDien.StartsWith("/uploads/avatars/"))
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, user.AnhDaiDien.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting old avatar for user {UserId}", userId);
                // Không throw exception vì đây không phải critical error
            }
        }
    }
}
