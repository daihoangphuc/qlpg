using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace GymManagement.Web.Data.Models
{
    public class VaiTro : IdentityRole<int>
    {
        [Required]
        [StringLength(50)]
        public string TenVaiTro { get; set; } = null!;

        [StringLength(200)]
        public string? MoTa { get; set; }

        // Navigation properties
        public virtual ICollection<TaiKhoan> TaiKhoans { get; set; } = new List<TaiKhoan>();
    }
}
