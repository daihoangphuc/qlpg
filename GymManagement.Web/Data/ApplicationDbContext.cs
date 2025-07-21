using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GymManagement.Web.Data.Identity;
using GymManagement.Web.Data.Models;

namespace GymManagement.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Gym Management DbSets
        public DbSet<VaiTro> VaiTros { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<GoiTap> GoiTaps { get; set; }
        public DbSet<LopHoc> LopHocs { get; set; }
        public DbSet<LichLop> LichLops { get; set; }
        public DbSet<KhuyenMai> KhuyenMais { get; set; }
        public DbSet<DangKy> DangKys { get; set; }
        public DbSet<ThanhToan> ThanhToans { get; set; }
        public DbSet<ThanhToanGateway> ThanhToanGateways { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BuoiHlv> BuoiHlvs { get; set; }
        public DbSet<BuoiTap> BuoiTaps { get; set; }
        public DbSet<MauMat> MauMats { get; set; }
        public DbSet<DiemDanh> DiemDanhs { get; set; }
        public DbSet<CauHinhHoaHong> CauHinhHoaHongs { get; set; }
        public DbSet<BangLuong> BangLuongs { get; set; }
        public DbSet<ThongBao> ThongBaos { get; set; }
        public DbSet<LichSuAnh> LichSuAnhs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Identity tables with custom names
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("AspNetUsers");
                entity.HasOne(u => u.NguoiDung)
                    .WithMany()
                    .HasForeignKey(u => u.NguoiDungId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ApplicationRole>(entity =>
            {
                entity.ToTable("AspNetRoles");
            });

            // Configure Gym Management entities (same as GymDbContext)
            ConfigureGymEntities(modelBuilder);
        }

        private void ConfigureGymEntities(ModelBuilder modelBuilder)
        {
            // Cấu hình bảng VaiTro
            modelBuilder.Entity<VaiTro>(entity =>
            {
                entity.HasKey(e => e.VaiTroId);
                entity.Property(e => e.VaiTroId).ValueGeneratedOnAdd();
                entity.Property(e => e.TenVaiTro).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.TenVaiTro).IsUnique();
                entity.Property(e => e.MoTa).HasMaxLength(200);
            });

            // Cấu hình bảng NguoiDung
            modelBuilder.Entity<NguoiDung>(entity =>
            {
                entity.HasKey(e => e.NguoiDungId);
                entity.Property(e => e.NguoiDungId).ValueGeneratedOnAdd();
                entity.Property(e => e.LoaiNguoiDung).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Ho).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Ten).HasMaxLength(50);
                entity.Property(e => e.GioiTinh).HasMaxLength(10);
                entity.Property(e => e.SoDienThoai).HasMaxLength(15);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.NgayThamGia).HasDefaultValueSql("date('now')");
                entity.Property(e => e.TrangThai).HasMaxLength(20).HasDefaultValue("ACTIVE");
            });

            // Cấu hình bảng TaiKhoan
            modelBuilder.Entity<TaiKhoan>(entity =>
            {
                entity.HasKey(e => e.TaiKhoanId);
                entity.Property(e => e.TaiKhoanId).ValueGeneratedOnAdd();
                entity.Property(e => e.TenDangNhap).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.TenDangNhap).IsUnique();
                entity.Property(e => e.MatKhauHash).HasMaxLength(255).IsRequired();
                entity.Property(e => e.KichHoat).HasDefaultValue(true);
                
                entity.HasOne(d => d.VaiTro)
                    .WithMany(p => p.TaiKhoans)
                    .HasForeignKey(d => d.VaiTroId);
                    
                entity.HasOne(d => d.NguoiDung)
                    .WithMany(p => p.TaiKhoans)
                    .HasForeignKey(d => d.NguoiDungId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình bảng GoiTap
            modelBuilder.Entity<GoiTap>(entity =>
            {
                entity.HasKey(e => e.GoiTapId);
                entity.Property(e => e.GoiTapId).ValueGeneratedOnAdd();
                entity.Property(e => e.TenGoi).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Gia).HasColumnType("decimal(12,2)").IsRequired();
                entity.Property(e => e.MoTa).HasMaxLength(500);
            });

            // Cấu hình bảng LopHoc
            modelBuilder.Entity<LopHoc>(entity =>
            {
                entity.HasKey(e => e.LopHocId);
                entity.Property(e => e.LopHocId).ValueGeneratedOnAdd();
                entity.Property(e => e.TenLop).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ThuTrongTuan).HasMaxLength(50).IsRequired();
                entity.Property(e => e.GiaTuyChinh).HasColumnType("decimal(12,2)");
                entity.Property(e => e.TrangThai).HasMaxLength(20).HasDefaultValue("OPEN");
                
                entity.HasOne(d => d.Hlv)
                    .WithMany(p => p.LopHocs)
                    .HasForeignKey(d => d.HlvId);
            });

            // Cấu hình bảng LichLop
            modelBuilder.Entity<LichLop>(entity =>
            {
                entity.HasKey(e => e.LichLopId);
                entity.Property(e => e.LichLopId).ValueGeneratedOnAdd();
                entity.Property(e => e.TrangThai).HasMaxLength(20).HasDefaultValue("SCHEDULED");
                
                entity.HasOne(d => d.LopHoc)
                    .WithMany(p => p.LichLops)
                    .HasForeignKey(d => d.LopHocId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình bảng KhuyenMai
            modelBuilder.Entity<KhuyenMai>(entity =>
            {
                entity.HasKey(e => e.KhuyenMaiId);
                entity.Property(e => e.KhuyenMaiId).ValueGeneratedOnAdd();
                entity.Property(e => e.MaCode).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.MaCode).IsUnique();
                entity.Property(e => e.MoTa).HasMaxLength(300);
                entity.Property(e => e.KichHoat).HasDefaultValue(true);
            });

            // Cấu hình bảng DangKy
            modelBuilder.Entity<DangKy>(entity =>
            {
                entity.HasKey(e => e.DangKyId);
                entity.Property(e => e.DangKyId).ValueGeneratedOnAdd();
                entity.Property(e => e.TrangThai).HasMaxLength(20).HasDefaultValue("ACTIVE");
                entity.Property(e => e.NgayTao).HasDefaultValueSql("datetime('now')");
                
                entity.HasOne(d => d.NguoiDung)
                    .WithMany(p => p.DangKys)
                    .HasForeignKey(d => d.NguoiDungId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(d => d.GoiTap)
                    .WithMany(p => p.DangKys)
                    .HasForeignKey(d => d.GoiTapId);
                    
                entity.HasOne(d => d.LopHoc)
                    .WithMany(p => p.DangKys)
                    .HasForeignKey(d => d.LopHocId);
            });

            // Continue with other entity configurations...
            ConfigureRemainingEntities(modelBuilder);
        }

        private void ConfigureRemainingEntities(ModelBuilder modelBuilder)
        {
            // Cấu hình bảng ThanhToan
            modelBuilder.Entity<ThanhToan>(entity =>
            {
                entity.HasKey(e => e.ThanhToanId);
                entity.Property(e => e.ThanhToanId).ValueGeneratedOnAdd();
                entity.Property(e => e.SoTien).HasColumnType("decimal(12,2)").IsRequired();
                entity.Property(e => e.NgayThanhToan).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.PhuongThuc).HasMaxLength(20);
                entity.Property(e => e.TrangThai).HasMaxLength(20).HasDefaultValue("PENDING");
                entity.Property(e => e.GhiChu).HasMaxLength(200);
                
                entity.HasOne(d => d.DangKy)
                    .WithMany(p => p.ThanhToans)
                    .HasForeignKey(d => d.DangKyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình bảng ThanhToanGateway
            modelBuilder.Entity<ThanhToanGateway>(entity =>
            {
                entity.HasKey(e => e.GatewayId);
                entity.Property(e => e.GatewayId).ValueGeneratedOnAdd();
                entity.Property(e => e.GatewayTen).HasMaxLength(30).HasDefaultValue("VNPAY");
                entity.Property(e => e.GatewayTransId).HasMaxLength(100);
                entity.Property(e => e.GatewayOrderId).HasMaxLength(100);
                entity.Property(e => e.GatewayAmount).HasColumnType("decimal(12,2)");
                entity.Property(e => e.GatewayRespCode).HasMaxLength(10);
                entity.Property(e => e.GatewayMessage).HasMaxLength(255);
                
                entity.HasOne(d => d.ThanhToan)
                    .WithOne(p => p.ThanhToanGateway)
                    .HasForeignKey<ThanhToanGateway>(d => d.ThanhToanId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure other entities similarly...
            // (Booking, BuoiHlv, BuoiTap, MauMat, DiemDanh, CauHinhHoaHong, BangLuong, ThongBao, LichSuAnh)
        }
    }
}
