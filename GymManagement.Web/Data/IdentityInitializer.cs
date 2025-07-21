using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GymManagement.Web.Data.Identity;
using GymManagement.Web.Data.Models;

namespace GymManagement.Web.Data
{
    public static class IdentityInitializer
    {
        public static async Task InitializeAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Create roles if they don't exist
            await CreateRolesAsync(roleManager);

            // Create admin user if it doesn't exist
            await CreateAdminUserAsync(userManager, context);

            // Sync existing TaiKhoan with Identity users
            await SyncExistingAccountsAsync(userManager, roleManager, context);
        }

        private static async Task CreateRolesAsync(RoleManager<ApplicationRole> roleManager)
        {
            var roles = new[]
            {
                new ApplicationRole("Admin") { Description = "Quản trị viên hệ thống" },
                new ApplicationRole("Manager") { Description = "Quản lý phòng gym" },
                new ApplicationRole("Staff") { Description = "Nhân viên" },
                new ApplicationRole("Trainer") { Description = "Huấn luyện viên" },
                new ApplicationRole("Member") { Description = "Thành viên" },
                new ApplicationRole("Guest") { Description = "Khách vãng lai" }
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role.Name!))
                {
                    await roleManager.CreateAsync(role);
                }
            }
        }

        private static async Task CreateAdminUserAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            const string adminEmail = "admin@gym.com";
            const string adminUserName = "admin";
            const string adminPassword = "Admin@123";

            var adminUser = await userManager.FindByNameAsync(adminUserName);
            if (adminUser == null)
            {
                // Find or create admin NguoiDung
                var adminNguoiDung = await context.NguoiDungs
                    .FirstOrDefaultAsync(x => x.Email == adminEmail);

                if (adminNguoiDung == null)
                {
                    adminNguoiDung = new NguoiDung
                    {
                        LoaiNguoiDung = "NHANVIEN",
                        Ho = "System",
                        Ten = "Administrator",
                        GioiTinh = "Nam",
                        NgaySinh = new DateOnly(1990, 1, 1),
                        SoDienThoai = "0123456789",
                        Email = adminEmail,
                        NgayThamGia = DateOnly.FromDateTime(DateTime.Now),
                        TrangThai = "ACTIVE"
                    };

                    context.NguoiDungs.Add(adminNguoiDung);
                    await context.SaveChangesAsync();
                }

                // Create admin ApplicationUser
                adminUser = new ApplicationUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    PhoneNumber = "0123456789",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    NguoiDungId = adminNguoiDung.NguoiDungId,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    await userManager.AddToRoleAsync(adminUser, "Manager");
                }
            }
        }

        private static async Task SyncExistingAccountsAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context)
        {
            // Get existing TaiKhoan records that don't have corresponding ApplicationUser
            var existingAccounts = await context.TaiKhoans
                .Include(t => t.NguoiDung)
                .Include(t => t.VaiTro)
                .Where(t => t.KichHoat)
                .ToListAsync();

            foreach (var taiKhoan in existingAccounts)
            {
                if (taiKhoan.NguoiDung == null) continue;

                // Check if ApplicationUser already exists
                var existingUser = await userManager.FindByNameAsync(taiKhoan.TenDangNhap);
                if (existingUser != null) continue;

                // Create ApplicationUser from TaiKhoan
                var applicationUser = new ApplicationUser
                {
                    UserName = taiKhoan.TenDangNhap,
                    Email = taiKhoan.NguoiDung.Email,
                    PhoneNumber = taiKhoan.NguoiDung.SoDienThoai,
                    EmailConfirmed = !string.IsNullOrEmpty(taiKhoan.NguoiDung.Email),
                    PhoneNumberConfirmed = !string.IsNullOrEmpty(taiKhoan.NguoiDung.SoDienThoai),
                    NguoiDungId = taiKhoan.NguoiDungId,
                    IsActive = taiKhoan.KichHoat
                };

                // Use the existing hashed password from TaiKhoan
                // Note: This assumes the password was hashed with BCrypt
                // In a real scenario, you might need to migrate passwords differently
                var result = await userManager.CreateAsync(applicationUser);
                if (result.Succeeded)
                {
                    // Set the password hash directly (bypass Identity's password hashing)
                    applicationUser.PasswordHash = taiKhoan.MatKhauHash;
                    await userManager.UpdateAsync(applicationUser);

                    // Assign role based on VaiTro or LoaiNguoiDung
                    var roleName = GetRoleNameFromVaiTroOrLoaiNguoiDung(
                        taiKhoan.VaiTro?.TenVaiTro, 
                        taiKhoan.NguoiDung.LoaiNguoiDung);
                    
                    if (!string.IsNullOrEmpty(roleName) && await roleManager.RoleExistsAsync(roleName))
                    {
                        await userManager.AddToRoleAsync(applicationUser, roleName);
                    }
                }
            }
        }

        private static string GetRoleNameFromVaiTroOrLoaiNguoiDung(string? vaiTroTen, string loaiNguoiDung)
        {
            // Priority: VaiTro name first, then LoaiNguoiDung
            if (!string.IsNullOrEmpty(vaiTroTen))
            {
                return vaiTroTen switch
                {
                    "Admin" => "Admin",
                    "Manager" => "Manager",
                    "Staff" => "Staff",
                    "Trainer" => "Trainer",
                    "Member" => "Member",
                    _ => GetRoleFromLoaiNguoiDung(loaiNguoiDung)
                };
            }

            return GetRoleFromLoaiNguoiDung(loaiNguoiDung);
        }

        private static string GetRoleFromLoaiNguoiDung(string loaiNguoiDung)
        {
            return loaiNguoiDung switch
            {
                "NHANVIEN" => "Staff",
                "HLV" => "Trainer",
                "THANHVIEN" => "Member",
                "VANGLAI" => "Guest",
                _ => "Member"
            };
        }
    }
}
