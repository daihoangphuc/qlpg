using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.Data.Models
{
    public class DiemDanh
    {
        public int DiemDanhId { get; set; }
        
        public int? ThanhVienId { get; set; }
        
        public DateTime ThoiGian { get; set; }

        public bool? KetQuaNhanDang { get; set; }

        [StringLength(255)]
        public string? AnhMinhChung { get; set; }

        public DateTime ThoiGianCheckIn { get; set; } = DateTime.Now;

        public int? LichLopId { get; set; }

        [StringLength(20)]
        public string TrangThai { get; set; } = "Present"; // Present, Absent, Late

        [StringLength(500)]
        public string? GhiChu { get; set; }

        // Navigation properties
        public virtual NguoiDung? ThanhVien { get; set; }
        public virtual LichLop? LichLop { get; set; }
    }
}
