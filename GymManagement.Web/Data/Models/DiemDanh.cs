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

        // Navigation properties
        public virtual NguoiDung? ThanhVien { get; set; }
    }
}
