using Microsoft.AspNetCore.Identity;
using GymManagement.Web.Data.Identity;
using GymManagement.Web.Models.DTOs;
using GymManagement.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResultDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(loginDto.UserName);
                if (user == null || !user.IsActive)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Tên đăng nhập hoặc mật khẩu không chính xác"
                    };
                }

                var result = await _signInManager.PasswordSignInAsync(
                    user, loginDto.Password, loginDto.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    // Update last login time
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    var userInfo = await GetUserInfoAsync(user);
                    return new AuthResultDto
                    {
                        Success = true,
                        Message = "Đăng nhập thành công",
                        User = userInfo
                    };
                }

                if (result.IsLockedOut)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Tài khoản đã bị khóa do đăng nhập sai quá nhiều lần"
                    };
                }

                if (result.RequiresTwoFactor)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Yêu cầu xác thực hai yếu tố"
                    };
                }

                return new AuthResultDto
                {
                    Success = false,
                    Message = "Tên đăng nhập hoặc mật khẩu không chính xác"
                };
            }
            catch (Exception ex)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra trong quá trình đăng nhập",
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<AuthResultDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Check if username already exists
                var existingUser = await _userManager.FindByNameAsync(registerDto.UserName);
                if (existingUser != null)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Tên đăng nhập đã tồn tại"
                    };
                }

                // Check if email already exists
                existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Email đã được sử dụng"
                    };
                }

                // Create NguoiDung first
                var nguoiDung = new Data.Models.NguoiDung
                {
                    LoaiNguoiDung = registerDto.LoaiNguoiDung,
                    Ho = registerDto.Ho,
                    Ten = registerDto.Ten,
                    GioiTinh = registerDto.GioiTinh,
                    NgaySinh = registerDto.NgaySinh,
                    SoDienThoai = registerDto.PhoneNumber,
                    Email = registerDto.Email,
                    NgayThamGia = DateOnly.FromDateTime(DateTime.Now),
                    TrangThai = "ACTIVE"
                };

                await _unitOfWork.NguoiDungs.AddAsync(nguoiDung);
                await _unitOfWork.SaveChangesAsync();

                // Create ApplicationUser
                var user = new ApplicationUser
                {
                    UserName = registerDto.UserName,
                    Email = registerDto.Email,
                    PhoneNumber = registerDto.PhoneNumber,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = !string.IsNullOrEmpty(registerDto.PhoneNumber),
                    NguoiDungId = nguoiDung.NguoiDungId,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (result.Succeeded)
                {
                    // Assign default role based on LoaiNguoiDung
                    var roleName = GetRoleNameFromLoaiNguoiDung(registerDto.LoaiNguoiDung);
                    if (!string.IsNullOrEmpty(roleName))
                    {
                        await _userManager.AddToRoleAsync(user, roleName);
                    }

                    var userInfo = await GetUserInfoAsync(user);
                    return new AuthResultDto
                    {
                        Success = true,
                        Message = "Đăng ký thành công",
                        User = userInfo
                    };
                }

                // If user creation failed, remove the NguoiDung
                await _unitOfWork.NguoiDungs.DeleteAsync(nguoiDung);
                await _unitOfWork.SaveChangesAsync();

                return new AuthResultDto
                {
                    Success = false,
                    Message = "Đăng ký thất bại",
                    Errors = result.Errors.Select(e => e.Description)
                };
            }
            catch (Exception ex)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra trong quá trình đăng ký",
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<AuthResultDto> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await GetCurrentApplicationUserAsync();
                if (user == null)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Người dùng không tồn tại"
                    };
                }

                var result = await _userManager.ChangePasswordAsync(user, 
                    changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

                if (result.Succeeded)
                {
                    return new AuthResultDto
                    {
                        Success = true,
                        Message = "Đổi mật khẩu thành công"
                    };
                }

                return new AuthResultDto
                {
                    Success = false,
                    Message = "Đổi mật khẩu thất bại",
                    Errors = result.Errors.Select(e => e.Description)
                };
            }
            catch (Exception ex)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra trong quá trình đổi mật khẩu",
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null || !user.IsActive)
                {
                    return false; // Don't reveal that the user does not exist
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                // TODO: Send email with reset token
                // For now, just return true
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<AuthResultDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
                if (user == null || !user.IsActive)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Người dùng không tồn tại"
                    };
                }

                var result = await _userManager.ResetPasswordAsync(user, 
                    resetPasswordDto.Token, resetPasswordDto.NewPassword);

                if (result.Succeeded)
                {
                    return new AuthResultDto
                    {
                        Success = true,
                        Message = "Đặt lại mật khẩu thành công"
                    };
                }

                return new AuthResultDto
                {
                    Success = false,
                    Message = "Đặt lại mật khẩu thất bại",
                    Errors = result.Errors.Select(e => e.Description)
                };
            }
            catch (Exception ex)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Có lỗi xảy ra trong quá trình đặt lại mật khẩu",
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<UserInfoDto?> GetCurrentUserAsync()
        {
            var user = await GetCurrentApplicationUserAsync();
            return user != null ? await GetUserInfoAsync(user) : null;
        }

        public async Task<bool> IsInRoleAsync(int userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user != null && await _userManager.IsInRoleAsync(user, role);
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user != null ? await _userManager.GetRolesAsync(user) : new List<string>();
        }

        public async Task<bool> LockUserAsync(int userId, DateTimeOffset? lockoutEnd = null)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null) return false;

                var result = await _userManager.SetLockoutEndDateAsync(user, 
                    lockoutEnd ?? DateTimeOffset.UtcNow.AddYears(100));
                return result.Succeeded;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UnlockUserAsync(int userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null) return false;

                var result = await _userManager.SetLockoutEndDateAsync(user, null);
                return result.Succeeded;
            }
            catch
            {
                return false;
            }
        }

        // Private helper methods
        private async Task<ApplicationUser?> GetCurrentApplicationUserAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                return await _userManager.GetUserAsync(httpContext.User);
            }
            return null;
        }

        private async Task<UserInfoDto> GetUserInfoAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var nguoiDung = user.NguoiDungId.HasValue 
                ? await _unitOfWork.NguoiDungs.GetByIdAsync(user.NguoiDungId.Value)
                : null;

            return new UserInfoDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                AccessFailedCount = user.AccessFailedCount,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive,
                Roles = roles,
                NguoiDungId = user.NguoiDungId,
                NguoiDung = nguoiDung != null ? MapNguoiDungToDto(nguoiDung) : null
            };
        }

        private static NguoiDungDto MapNguoiDungToDto(Data.Models.NguoiDung nguoiDung)
        {
            return new NguoiDungDto
            {
                NguoiDungId = nguoiDung.NguoiDungId,
                LoaiNguoiDung = nguoiDung.LoaiNguoiDung,
                Ho = nguoiDung.Ho,
                Ten = nguoiDung.Ten,
                GioiTinh = nguoiDung.GioiTinh,
                NgaySinh = nguoiDung.NgaySinh,
                SoDienThoai = nguoiDung.SoDienThoai,
                Email = nguoiDung.Email,
                NgayThamGia = nguoiDung.NgayThamGia,
                TrangThai = nguoiDung.TrangThai
            };
        }

        private static string GetRoleNameFromLoaiNguoiDung(string loaiNguoiDung)
        {
            return loaiNguoiDung switch
            {
                "NHANVIEN" => "Staff",
                "HLV" => "Trainer",
                "THANHVIEN" => "Member",
                "VANGLAI" => "Guest",
                _ => "Member"
            };
        }
    }
}
