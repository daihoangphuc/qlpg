using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.Data.Models
{
    public class CauHinhHoaHong
    {
        public int CauHinhHoaHongId { get; set; }
        
        public int? GoiTapId { get; set; }
        
        [Required]
        [Range(0, 100)]
        public int PhanTramHoaHong { get; set; }

        // Navigation properties
        public virtual GoiTap? GoiTap { get; set; }
    }
}
