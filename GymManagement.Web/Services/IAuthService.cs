using GymManagement.Web.Models.DTOs;

namespace GymManagement.Web.Services
{
    public interface IAuthService
    {
        Task<AuthResultDto> LoginAsync(LoginDto loginDto);
        Task<AuthResultDto> RegisterAsync(RegisterDto registerDto);
        Task<bool> LogoutAsync();
        Task<AuthResultDto> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
        Task<bool> ForgotPasswordAsync(string email);
        Task<AuthResultDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<UserInfoDto?> GetCurrentUserAsync();
        Task<bool> IsInRoleAsync(int userId, string role);
        Task<IEnumerable<string>> GetUserRolesAsync(int userId);
        Task<bool> LockUserAsync(int userId, DateTimeOffset? lockoutEnd = null);
        Task<bool> UnlockUserAsync(int userId);
    }
}
