using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Web.Data.Models
{
    public class BangLuong
    {
        public int BangLuongId { get; set; }
        
        public int? HlvId { get; set; }
        
        [Required]
        [StringLength(7)] // YYYY-MM
        public string Thang { get; set; } = null!;
        
        [Required]
        [Column(TypeName = "decimal(12,2)")]
        public decimal LuongCoBan { get; set; }
        
        [Column(TypeName = "decimal(12,2)")]
        public decimal TienHoaHong { get; set; } = 0; // Total commission

        [NotMapped]
        public decimal TongThanhToan => LuongCoBan + TienHoaHong;

        public DateOnly? NgayThanhToan { get; set; }

        [StringLength(500)]
        public string? GhiChu { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual NguoiDung? Hlv { get; set; }
    }
}
