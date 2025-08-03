using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Web.Data.Models
{
    public class CauHinhHoaHong
    {
        public int CauHinhHoaHongId { get; set; }
        
        public int? GoiTapId { get; set; }
        
        public int? LopHocId { get; set; }
        
        [Required]
        [Range(0, 100)]
        public int PhanTramHoaHong { get; set; } // Basic commission percentage

        public bool KichHoat { get; set; } = true;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? MoTa { get; set; }

        // Navigation properties
        public virtual GoiTap? GoiTap { get; set; }
        public virtual LopHoc? LopHoc { get; set; }
    }
}
