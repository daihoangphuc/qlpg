using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.Data.Models
{
    public class TaiKhoan
    {
        public int TaiKhoanId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string TenDangNhap { get; set; } = null!;
        
        [Required]
        [StringLength(255)]
        public string MatKhauHash { get; set; } = null!;
        
        public int? VaiTroId { get; set; }
        
        public int NguoiDungId { get; set; }
        
        public bool KichHoat { get; set; } = true;

        // Navigation properties
        public virtual VaiTro? VaiTro { get; set; }
        public virtual NguoiDung NguoiDung { get; set; } = null!;
    }
}
