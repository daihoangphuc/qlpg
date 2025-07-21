using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace GymManagement.Web.Data.Models
{
    public class TaiKhoan : IdentityUser<int>
    {
        [StringLength(50)]
        public string TenDangNhap { get; set; } = null!;

        public int? VaiTroId { get; set; }

        public int? NguoiDungId { get; set; }

        public bool KichHoat { get; set; } = true;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual VaiTro? VaiTro { get; set; }
        public virtual NguoiDung? NguoiDung { get; set; }
    }
}
