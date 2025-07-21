using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.Data.Models
{
    public class DangKy
    {
        public int DangKyId { get; set; }
        
        [Required]
        public int NguoiDungId { get; set; }
        
        public int? GoiTapId { get; set; }
        
        public int? LopHocId { get; set; }
        
        [Required]
        public DateOnly NgayBatDau { get; set; }
        
        [Required]
        public DateOnly NgayKetThuc { get; set; }
        
        [StringLength(20)]
        public string TrangThai { get; set; } = "ACTIVE";
        
        public DateTime NgayTao { get; set; }

        // Navigation properties
        public virtual NguoiDung NguoiDung { get; set; } = null!;
        public virtual GoiTap? GoiTap { get; set; }
        public virtual LopHoc? LopHoc { get; set; }
        public virtual ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();
    }
}
