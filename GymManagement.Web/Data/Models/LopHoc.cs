using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Web.Data.Models
{
    public class LopHoc
    {
        public int LopHocId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string TenLop { get; set; } = null!;
        
        public int? HlvId { get; set; }
        
        [Required]
        public int SucChua { get; set; }
        
        [Required]
        public TimeOnly GioBatDau { get; set; }
        
        [Required]
        public TimeOnly GioKetThuc { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ThuTrongTuan { get; set; } = null!;
        
        [Column(TypeName = "decimal(12,2)")]
        public decimal? GiaTuyChinh { get; set; }
        
        [StringLength(20)]
        public string TrangThai { get; set; } = "OPEN";

        // Navigation properties
        public virtual NguoiDung? Hlv { get; set; }
        public virtual ICollection<LichLop> LichLops { get; set; } = new List<LichLop>();
        public virtual ICollection<DangKy> DangKys { get; set; } = new List<DangKy>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<BuoiHlv> BuoiHlvs { get; set; } = new List<BuoiHlv>();
        public virtual ICollection<BuoiTap> BuoiTaps { get; set; } = new List<BuoiTap>();
    }
}
