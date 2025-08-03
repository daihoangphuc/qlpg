using GymManagement.Web.Data.Models;

namespace GymManagement.Web.Services
{
    public interface IGoogleAuthService
    {
        Task<(bool success, string message, TaiKhoan? user)> AuthenticateGoogleUserAsync(string email, string name, string googleId);
        Task<TaiKhoan?> GetUserByGoogleIdAsync(string googleId);
        Task<TaiKhoan?> GetUserByEmailAsync(string email);
        Task<bool> LinkGoogleAccountAsync(int userId, string googleId);
        Task<bool> UnlinkGoogleAccountAsync(int userId);
        Task<bool> IsGoogleAccountLinkedAsync(int userId);
    }
}