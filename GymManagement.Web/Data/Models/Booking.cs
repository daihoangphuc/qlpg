using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.Data.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        
        public int? ThanhVienId { get; set; }
        
        public int? LopHocId { get; set; }
        
        public int? LichLopId { get; set; }
        
        [Required]
        public DateOnly Ngay { get; set; }
        
        [StringLength(20)]
        public string TrangThai { get; set; } = "BOOKED"; // BOOKED/CANCELED/ATTENDED

        // Navigation properties
        public virtual NguoiDung? ThanhVien { get; set; }
        public virtual LopHoc? LopHoc { get; set; }
        public virtual LichLop? LichLop { get; set; }
    }
}
