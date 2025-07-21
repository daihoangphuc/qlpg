using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.Models.DTOs
{
    public class NguoiDungDto
    {
        public int NguoiDungId { get; set; }
        
        [Required(ErrorMessage = "Loại người dùng là bắt buộc")]
        [Display(Name = "Loại người dùng")]
        public string LoaiNguoiDung { get; set; } = null!;
        
        [Required(ErrorMessage = "Họ là bắt buộc")]
        [Display(Name = "Họ")]
        [StringLength(50, ErrorMessage = "Họ không được vượt quá 50 ký tự")]
        public string Ho { get; set; } = null!;
        
        [Display(Name = "Tên")]
        [StringLength(50, ErrorMessage = "Tên không được vượt quá 50 ký tự")]
        public string? Ten { get; set; }
        
        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; }
        
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateOnly? NgaySinh { get; set; }
        
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }
        
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }
        
        [Display(Name = "Ngày tham gia")]
        [DataType(DataType.Date)]
        public DateOnly NgayThamGia { get; set; }
        
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "ACTIVE";

        [Display(Name = "Họ và tên")]
        public string HoTen => $"{Ho} {Ten}".Trim();
    }

    public class CreateNguoiDungDto
    {
        [Required(ErrorMessage = "Loại người dùng là bắt buộc")]
        [Display(Name = "Loại người dùng")]
        public string LoaiNguoiDung { get; set; } = null!;
        
        [Required(ErrorMessage = "Họ là bắt buộc")]
        [Display(Name = "Họ")]
        [StringLength(50, ErrorMessage = "Họ không được vượt quá 50 ký tự")]
        public string Ho { get; set; } = null!;
        
        [Display(Name = "Tên")]
        [StringLength(50, ErrorMessage = "Tên không được vượt quá 50 ký tự")]
        public string? Ten { get; set; }
        
        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; }
        
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateOnly? NgaySinh { get; set; }
        
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }
        
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }
    }

    public class UpdateNguoiDungDto
    {
        public int NguoiDungId { get; set; }
        
        [Required(ErrorMessage = "Loại người dùng là bắt buộc")]
        [Display(Name = "Loại người dùng")]
        public string LoaiNguoiDung { get; set; } = null!;
        
        [Required(ErrorMessage = "Họ là bắt buộc")]
        [Display(Name = "Họ")]
        [StringLength(50, ErrorMessage = "Họ không được vượt quá 50 ký tự")]
        public string Ho { get; set; } = null!;
        
        [Display(Name = "Tên")]
        [StringLength(50, ErrorMessage = "Tên không được vượt quá 50 ký tự")]
        public string? Ten { get; set; }
        
        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; }
        
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateOnly? NgaySinh { get; set; }
        
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }
        
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }
        
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "ACTIVE";
    }
}
