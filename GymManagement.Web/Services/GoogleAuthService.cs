using GymManagement.Web.Data;
using GymManagement.Web.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace GymManagement.Web.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly GymDbContext _context;
        private readonly UserManager<TaiKhoan> _userManager;
        private readonly ILogger<GoogleAuthService> _logger;

        public GoogleAuthService(GymDbContext context, UserManager<TaiKhoan> userManager, ILogger<GoogleAuthService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<(bool success, string message, TaiKhoan? user)> AuthenticateGoogleUserAsync(string email, string name, string googleId)
        {
            try
            {
                // Kiểm tra xem email đã tồn tại chưa
                var existingUser = await GetUserByEmailAsync(email);
                
                if (existingUser != null)
                {
                    // Nếu user đã tồn tại, kiểm tra xem có link với Google chưa
                    var isLinked = await IsGoogleAccountLinkedAsync(existingUser.TaiKhoanId);
                    
                    if (!isLinked)
                    {
                        // Link Google account với user hiện tại
                        await LinkGoogleAccountAsync(existingUser.TaiKhoanId, googleId);
                        _logger.LogInformation("Linked Google account to existing user: {Email}", email);
                    }
                    
                    return (true, "Đăng nhập thành công với tài khoản hiện tại", existingUser);
                }

                // Tạo user mới nếu chưa tồn tại
                var newUser = new TaiKhoan
                {
                    TenDangNhap = email,
                    MatKhauHash = "", // Không cần password cho Google login
                    KichHoat = true,
                    NguoiDung = new NguoiDung
                    {
                        LoaiNguoiDung = "THANHVIEN",
                        Ho = name.Split(' ').FirstOrDefault() ?? "",
                        Ten = string.Join(" ", name.Split(' ').Skip(1)),
                        Email = email,
                        NgayThamGia = DateOnly.FromDateTime(DateTime.Now),
                        TrangThai = "ACTIVE"
                    }
                };

                // Tạo user trong Identity
                var result = await _userManager.CreateAsync(newUser);
                
                if (result.Succeeded)
                {
                    // Link Google account
                    await LinkGoogleAccountAsync(newUser.TaiKhoanId, googleId);
                    
                    // Gán role mặc định
                    await _userManager.AddToRoleAsync(newUser, "Member");
                    
                    _logger.LogInformation("Created new user with Google authentication: {Email}", email);
                    return (true, "Tạo tài khoản mới thành công", newUser);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create user: {Errors}", errors);
                    return (false, $"Lỗi tạo tài khoản: {errors}", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Google authentication for email: {Email}", email);
                return (false, "Lỗi xác thực Google", null);
            }
        }

        public async Task<TaiKhoan?> GetUserByGoogleIdAsync(string googleId)
        {
            // Tìm user theo Google ID thông qua ExternalLogin
            var externalLogin = await _context.ExternalLogins
                .Include(el => el.TaiKhoan)
                .ThenInclude(t => t.NguoiDung)
                .FirstOrDefaultAsync(el => el.Provider == "Google" && el.ProviderKey == googleId);
            
            return externalLogin?.TaiKhoan;
        }

        public async Task<TaiKhoan?> GetUserByEmailAsync(string email)
        {
            return await _context.TaiKhoans
                .Include(t => t.NguoiDung)
                .FirstOrDefaultAsync(t => t.TenDangNhap == email);
        }

        public async Task<bool> LinkGoogleAccountAsync(int userId, string googleId)
        {
            try
            {
                var user = await _context.TaiKhoans.FindAsync(userId.ToString());
                if (user != null)
                {
                    // Kiểm tra xem đã link Google chưa
                    var existingLink = await _context.ExternalLogins
                        .FirstOrDefaultAsync(el => el.TaiKhoanId == userId.ToString() && el.Provider == "Google");
                    
                    if (existingLink == null)
                    {
                        // Tạo link mới
                        var externalLogin = new ExternalLogin
                        {
                            TaiKhoanId = userId.ToString(),
                            Provider = "Google",
                            ProviderKey = googleId,
                            ProviderDisplayName = "Google"
                        };
                        
                        _context.ExternalLogins.Add(externalLogin);
                        await _context.SaveChangesAsync();
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking Google account for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> UnlinkGoogleAccountAsync(int userId)
        {
            try
            {
                var externalLogin = await _context.ExternalLogins
                    .FirstOrDefaultAsync(el => el.TaiKhoanId == userId.ToString() && el.Provider == "Google");
                
                if (externalLogin != null)
                {
                    _context.ExternalLogins.Remove(externalLogin);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlinking Google account for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> IsGoogleAccountLinkedAsync(int userId)
        {
            var externalLogin = await _context.ExternalLogins
                .FirstOrDefaultAsync(el => el.TaiKhoanId == userId.ToString() && el.Provider == "Google");
            return externalLogin != null;
        }
    }
}