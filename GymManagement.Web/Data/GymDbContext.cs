using Microsoft.EntityFrameworkCore;
using GymManagement.Web.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GymManagement.Web.Data
{
    public class GymDbContext : IdentityDbContext<TaiKhoan, VaiTro, int>
    {
        public GymDbContext(DbContextOptions<GymDbContext> options) : base(options)
        {
        }

        // Bảo mật
        public DbSet<VaiTro> VaiTros { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }

        // Sản phẩm - Khuyến mãi
        public DbSet<GoiTap> GoiTaps { get; set; }
        public DbSet<LopHoc> LopHocs { get; set; }
        public DbSet<LichLop> LichLops { get; set; }
        public DbSet<KhuyenMai> KhuyenMais { get; set; }

        // Đăng ký - Thanh toán
        public DbSet<DangKy> DangKys { get; set; }
        public DbSet<ThanhToan> ThanhToans { get; set; }
        public DbSet<ThanhToanGateway> ThanhToanGateways { get; set; }

        // Đặt chỗ
        public DbSet<Booking> Bookings { get; set; }

        // Hoạt động - Check-in
        public DbSet<BuoiHlv> BuoiHlvs { get; set; }
        public DbSet<BuoiTap> BuoiTaps { get; set; }
        public DbSet<MauMat> MauMats { get; set; }
        public DbSet<DiemDanh> DiemDanhs { get; set; }

        // Lương & Hoa hồng
        public DbSet<CauHinhHoaHong> CauHinhHoaHongs { get; set; }
        public DbSet<BangLuong> BangLuongs { get; set; }

        // Hệ thống
        public DbSet<ThongBao> ThongBaos { get; set; }
        public DbSet<LichSuAnh> LichSuAnhs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Rename Identity tables to match our schema
            modelBuilder.Entity<TaiKhoan>().ToTable("TaiKhoan");
            modelBuilder.Entity<VaiTro>().ToTable("VaiTro");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<int>>().ToTable("TaiKhoanClaims");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<int>>().ToTable("TaiKhoanLogins");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<int>>().ToTable("TaiKhoanTokens");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<int>>().ToTable("TaiKhoanVaiTros");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>>().ToTable("VaiTroClaims");

            // Cấu hình bảng VaiTro
            modelBuilder.Entity<VaiTro>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("VaiTroId");
                entity.Property(e => e.Name).HasColumnName("TenVaiTro").HasMaxLength(50).IsRequired();
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
                entity.Property(e => e.NgayThamGia).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.TrangThai).HasMaxLength(20).HasDefaultValue("ACTIVE");
            });

            // Cấu hình bảng TaiKhoan
            modelBuilder.Entity<TaiKhoan>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("TaiKhoanId");
                entity.Property(e => e.UserName).HasColumnName("TenDangNhap").HasMaxLength(50).IsRequired();
                entity.Property(e => e.TenDangNhap).HasMaxLength(50);
                entity.Property(e => e.KichHoat).HasDefaultValue(true);
                entity.Property(e => e.NgayTao).HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.VaiTro)
                    .WithMany(p => p.TaiKhoans)
                    .HasForeignKey(d => d.VaiTroId);

                entity.HasOne(d => d.NguoiDung)
                    .WithMany()
                    .HasForeignKey(d => d.NguoiDungId)
                    .OnDelete(DeleteBehavior.SetNull);
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

            // Cấu hình bảng Booking
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.BookingId);
                entity.Property(e => e.BookingId).ValueGeneratedOnAdd();
                entity.Property(e => e.TrangThai).HasMaxLength(20).HasDefaultValue("BOOKED");
                
                entity.HasOne(d => d.ThanhVien)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.ThanhVienId);
                    
                entity.HasOne(d => d.LopHoc)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.LopHocId);
                    
                entity.HasOne(d => d.LichLop)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.LichLopId);
            });

            // Cấu hình bảng BuoiHlv
            modelBuilder.Entity<BuoiHlv>(entity =>
            {
                entity.HasKey(e => e.BuoiHlvId);
                entity.Property(e => e.BuoiHlvId).ValueGeneratedOnAdd();
                entity.Property(e => e.GhiChu).HasMaxLength(300);
                
                entity.HasOne(d => d.Hlv)
                    .WithMany(p => p.BuoiHlvs)
                    .HasForeignKey(d => d.HlvId);
                    
                entity.HasOne(d => d.ThanhVien)
                    .WithMany(p => p.BuoiHlvThanhViens)
                    .HasForeignKey(d => d.ThanhVienId);
                    
                entity.HasOne(d => d.LopHoc)
                    .WithMany(p => p.BuoiHlvs)
                    .HasForeignKey(d => d.LopHocId);
            });

            // Cấu hình bảng BuoiTap
            modelBuilder.Entity<BuoiTap>(entity =>
            {
                entity.HasKey(e => e.BuoiTapId);
                entity.Property(e => e.BuoiTapId).ValueGeneratedOnAdd();
                entity.Property(e => e.GhiChu).HasMaxLength(200);
                
                entity.HasOne(d => d.ThanhVien)
                    .WithMany(p => p.BuoiTaps)
                    .HasForeignKey(d => d.ThanhVienId);
                    
                entity.HasOne(d => d.LopHoc)
                    .WithMany(p => p.BuoiTaps)
                    .HasForeignKey(d => d.LopHocId);
            });

            // Cấu hình bảng MauMat
            modelBuilder.Entity<MauMat>(entity =>
            {
                entity.HasKey(e => e.MauMatId);
                entity.Property(e => e.MauMatId).ValueGeneratedOnAdd();
                entity.Property(e => e.NgayTao).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.ThuatToan).HasMaxLength(50).HasDefaultValue("ArcFace");
                
                entity.HasOne(d => d.NguoiDung)
                    .WithOne(p => p.MauMat)
                    .HasForeignKey<MauMat>(d => d.NguoiDungId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình bảng DiemDanh
            modelBuilder.Entity<DiemDanh>(entity =>
            {
                entity.HasKey(e => e.DiemDanhId);
                entity.Property(e => e.DiemDanhId).ValueGeneratedOnAdd();
                entity.Property(e => e.ThoiGian).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.AnhMinhChung).HasMaxLength(255);
                
                entity.HasOne(d => d.ThanhVien)
                    .WithMany(p => p.DiemDanhs)
                    .HasForeignKey(d => d.ThanhVienId);
            });

            // Cấu hình bảng CauHinhHoaHong
            modelBuilder.Entity<CauHinhHoaHong>(entity =>
            {
                entity.HasKey(e => e.CauHinhHoaHongId);
                entity.Property(e => e.CauHinhHoaHongId).ValueGeneratedOnAdd();
                
                entity.HasOne(d => d.GoiTap)
                    .WithMany(p => p.CauHinhHoaHongs)
                    .HasForeignKey(d => d.GoiTapId);
            });

            // Cấu hình bảng BangLuong
            modelBuilder.Entity<BangLuong>(entity =>
            {
                entity.HasKey(e => e.BangLuongId);
                entity.Property(e => e.BangLuongId).ValueGeneratedOnAdd();
                entity.Property(e => e.Thang).HasMaxLength(7).IsRequired();
                entity.Property(e => e.LuongCoBan).HasColumnType("decimal(12,2)").IsRequired();
                entity.Property(e => e.TienHoaHong).HasColumnType("decimal(12,2)").HasDefaultValue(0);
                
                entity.HasOne(d => d.Hlv)
                    .WithMany(p => p.BangLuongs)
                    .HasForeignKey(d => d.HlvId);
            });

            // Cấu hình bảng ThongBao
            modelBuilder.Entity<ThongBao>(entity =>
            {
                entity.HasKey(e => e.ThongBaoId);
                entity.Property(e => e.ThongBaoId).ValueGeneratedOnAdd();
                entity.Property(e => e.TieuDe).HasMaxLength(100);
                entity.Property(e => e.NoiDung).HasMaxLength(1000);
                entity.Property(e => e.NgayTao).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.Kenh).HasMaxLength(10);
                entity.Property(e => e.DaDoc).HasDefaultValue(false);
                
                entity.HasOne(d => d.NguoiDung)
                    .WithMany(p => p.ThongBaos)
                    .HasForeignKey(d => d.NguoiDungId);
            });

            // Cấu hình bảng LichSuAnh
            modelBuilder.Entity<LichSuAnh>(entity =>
            {
                entity.HasKey(e => e.LichSuAnhId);
                entity.Property(e => e.LichSuAnhId).ValueGeneratedOnAdd();
                entity.Property(e => e.AnhCu).HasMaxLength(255);
                entity.Property(e => e.AnhMoi).HasMaxLength(255);
                entity.Property(e => e.NgayCapNhat).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LyDo).HasMaxLength(200);
                
                entity.HasOne(d => d.NguoiDung)
                    .WithMany(p => p.LichSuAnhs)
                    .HasForeignKey(d => d.NguoiDungId);
            });
        }
    }
}
