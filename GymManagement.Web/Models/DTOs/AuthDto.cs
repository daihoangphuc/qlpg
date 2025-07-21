using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.Models.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class RegisterDto
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [Display(Name = "Tên đăng nhập")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Display(Name = "Xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = null!;

        [Required(ErrorMessage = "Họ là bắt buộc")]
        [Display(Name = "Họ")]
        [StringLength(50, ErrorMessage = "Họ không được vượt quá 50 ký tự")]
        public string Ho { get; set; } = null!;

        [Display(Name = "Tên")]
        [StringLength(50, ErrorMessage = "Tên không được vượt quá 50 ký tự")]
        public string? Ten { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateOnly? NgaySinh { get; set; }

        [Required(ErrorMessage = "Loại người dùng là bắt buộc")]
        [Display(Name = "Loại người dùng")]
        public string LoaiNguoiDung { get; set; } = "THANHVIEN";
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc")]
        [Display(Name = "Mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [Display(Name = "Mật khẩu mới")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Xác nhận mật khẩu mới là bắt buộc")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmNewPassword { get; set; } = null!;
    }

    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mã xác nhận là bắt buộc")]
        [Display(Name = "Mã xác nhận")]
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [Display(Name = "Mật khẩu mới")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Xác nhận mật khẩu mới là bắt buộc")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmNewPassword { get; set; } = null!;
    }

    public class AuthResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public UserInfoDto? User { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();
    }

    public class UserInfoDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public int AccessFailedCount { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public IEnumerable<string> Roles { get; set; } = new List<string>();
        
        // Gym-specific properties
        public int? NguoiDungId { get; set; }
        public NguoiDungDto? NguoiDung { get; set; }
    }

    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;
    }
}
