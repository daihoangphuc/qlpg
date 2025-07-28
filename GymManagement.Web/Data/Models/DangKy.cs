using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public decimal? PhiDangKy { get; set; }

        [StringLength(500)]
        public string? LyDoHuy { get; set; }

        [StringLength(20)]
        public string LoaiDangKy { get; set; } = "PACKAGE"; // PACKAGE hoặc CLASS

        [StringLength(50)]
        public string? TrangThaiChiTiet { get; set; }

        // Alias properties for backward compatibility
        [NotMapped]
        public virtual NguoiDung? ThanhVien => NguoiDung;

        [NotMapped]
        public DateTime NgayDangKy => NgayTao;

        // Computed properties
        [NotMapped]
        public bool IsClassRegistration => LopHocId.HasValue;

        [NotMapped]
        public bool IsPackageRegistration => GoiTapId.HasValue;

        [NotMapped]
        public bool CanCancel => TrangThai == "ACTIVE" &&
            (IsPackageRegistration || (IsClassRegistration && NgayBatDau > DateOnly.FromDateTime(DateTime.Today.AddDays(1))));

        [NotMapped]
        public string DisplayType => IsClassRegistration ? "Lớp học" : "Gói tập";

        // Navigation properties
        public virtual NguoiDung NguoiDung { get; set; } = null!;
        public virtual GoiTap? GoiTap { get; set; }
        public virtual LopHoc? LopHoc { get; set; }
        public virtual ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();
    }
}
