using Microsoft.AspNetCore.Identity;
using GymManagement.Web.Data.Models;

namespace GymManagement.Web.Data.Identity
{
    public class ApplicationUser : IdentityUser<int>
    {
        public int? NguoiDungId { get; set; }
        public virtual NguoiDung? NguoiDung { get; set; }
        
        // Override properties to match our TaiKhoan table
        public override string? UserName { get; set; }
        public override string? Email { get; set; }
        public override string? PhoneNumber { get; set; }
        public override bool EmailConfirmed { get; set; } = true;
        public override bool PhoneNumberConfirmed { get; set; } = true;
        public override bool LockoutEnabled { get; set; } = true;
        public override DateTimeOffset? LockoutEnd { get; set; }
        public override int AccessFailedCount { get; set; }
        public override bool TwoFactorEnabled { get; set; } = false;
        
        // Additional properties
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
