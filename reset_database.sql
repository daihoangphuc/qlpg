-- Script để reset database và tạo lại cấu trúc đúng
USE master;
GO

-- Drop database if exists
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'GymManagementDb')
BEGIN
    ALTER DATABASE GymManagementDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE GymManagementDb;
END
GO

-- Create new database
CREATE DATABASE GymManagementDb;
GO

USE GymManagementDb;
GO

-- Tạo bảng VaiTro (Identity Roles)
CREATE TABLE VaiTro (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    Name NVARCHAR(256) NULL,
    NormalizedName NVARCHAR(256) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL,
    TenVaiTro NVARCHAR(50) NOT NULL,
    MoTa NVARCHAR(200) NULL
);

-- Tạo bảng NguoiDung
CREATE TABLE NguoiDung (
    NguoiDungId INT IDENTITY(1,1) PRIMARY KEY,
    LoaiNguoiDung NVARCHAR(20) NOT NULL,
    Ho NVARCHAR(50) NOT NULL,
    Ten NVARCHAR(50) NULL,
    GioiTinh NVARCHAR(10) NULL,
    NgaySinh DATE NULL,
    SoDienThoai NVARCHAR(15) NULL,
    Email NVARCHAR(100) NULL,
    DiaChi NVARCHAR(255) NULL,
    NgayThamGia DATE NOT NULL DEFAULT GETDATE(),
    TrangThai NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    AnhDaiDien NVARCHAR(255) NULL,
    NgayTao DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- Tạo bảng TaiKhoan (Identity Users)
CREATE TABLE TaiKhoan (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    UserName NVARCHAR(256) NULL,
    NormalizedUserName NVARCHAR(256) NULL,
    Email NVARCHAR(256) NULL,
    NormalizedEmail NVARCHAR(256) NULL,
    EmailConfirmed BIT NOT NULL,
    PasswordHash NVARCHAR(MAX) NULL,
    SecurityStamp NVARCHAR(MAX) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL,
    PhoneNumber NVARCHAR(MAX) NULL,
    PhoneNumberConfirmed BIT NOT NULL,
    TwoFactorEnabled BIT NOT NULL,
    LockoutEnd DATETIMEOFFSET(7) NULL,
    LockoutEnabled BIT NOT NULL,
    AccessFailedCount INT NOT NULL,
    TenDangNhap NVARCHAR(50) NOT NULL,
    NguoiDungId INT NULL,
    KichHoat BIT NOT NULL DEFAULT 1,
    NgayTao DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (NguoiDungId) REFERENCES NguoiDung(NguoiDungId) ON DELETE SET NULL
);

-- Tạo bảng TaiKhoanVaiTros (Identity UserRoles)
CREATE TABLE TaiKhoanVaiTros (
    UserId NVARCHAR(450) NOT NULL,
    RoleId NVARCHAR(450) NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES TaiKhoan(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES VaiTro(Id) ON DELETE CASCADE
);

-- Tạo các bảng khác
CREATE TABLE GoiTap (
    GoiTapId INT IDENTITY(1,1) PRIMARY KEY,
    TenGoiTap NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(500) NULL,
    Gia DECIMAL(18,2) NOT NULL,
    ThoiHan INT NOT NULL,
    TrangThai NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    NgayTao DATETIME2 NOT NULL DEFAULT GETDATE()
);

CREATE TABLE LopHoc (
    LopHocId INT IDENTITY(1,1) PRIMARY KEY,
    TenLop NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(500) NULL,
    HuanLuyenVienId INT NULL,
    ThoiGian NVARCHAR(100) NULL,
    SucChua INT NOT NULL DEFAULT 20,
    Gia DECIMAL(18,2) NOT NULL DEFAULT 0,
    TrangThai NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    NgayTao DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (HuanLuyenVienId) REFERENCES NguoiDung(NguoiDungId) ON DELETE SET NULL
);

CREATE TABLE DangKy (
    DangKyId INT IDENTITY(1,1) PRIMARY KEY,
    NguoiDungId INT NOT NULL,
    GoiTapId INT NULL,
    LopHocId INT NULL,
    NgayBatDau DATE NOT NULL,
    NgayKetThuc DATE NOT NULL,
    TrangThai NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    NgayTao DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (NguoiDungId) REFERENCES NguoiDung(NguoiDungId) ON DELETE CASCADE,
    FOREIGN KEY (GoiTapId) REFERENCES GoiTap(GoiTapId) ON DELETE SET NULL,
    FOREIGN KEY (LopHocId) REFERENCES LopHoc(LopHocId) ON DELETE SET NULL
);

-- Tạo indexes
CREATE UNIQUE INDEX IX_VaiTro_TenVaiTro ON VaiTro (TenVaiTro);
CREATE UNIQUE INDEX IX_TaiKhoan_TenDangNhap ON TaiKhoan (TenDangNhap);
CREATE INDEX IX_TaiKhoan_NguoiDungId ON TaiKhoan (NguoiDungId);
CREATE INDEX IX_DangKy_NguoiDungId ON DangKy (NguoiDungId);
CREATE INDEX IX_DangKy_GoiTapId ON DangKy (GoiTapId);
CREATE INDEX IX_DangKy_LopHocId ON DangKy (LopHocId);

-- Insert seed data
-- Vai trò (Simplified to 3 roles)
INSERT INTO VaiTro (Id, Name, NormalizedName, TenVaiTro, MoTa) VALUES
('1', 'Admin', 'ADMIN', 'Admin', 'Quản trị viên hệ thống (bao gồm Manager và Staff)'),
('2', 'Trainer', 'TRAINER', 'Trainer', 'Huấn luyện viên'),
('3', 'Member', 'MEMBER', 'Member', 'Thành viên');

-- Người dùng
INSERT INTO NguoiDung (LoaiNguoiDung, Ho, Ten, GioiTinh, NgaySinh, SoDienThoai, Email, DiaChi, NgayThamGia, TrangThai) VALUES
('ADMIN', 'Nguyễn Văn', 'Admin', 'Nam', '1990-01-01', '0123456789', 'admin@gym.com', 'Hà Nội', GETDATE(), 'ACTIVE'),
('TRAINER', 'Trần Thị', 'Hương', 'Nữ', '1995-05-15', '0987654321', 'huong.trainer@gym.com', 'TP.HCM', GETDATE(), 'ACTIVE'),
('TRAINER', 'Lê Minh', 'Tuấn', 'Nam', '1992-08-20', '0912345678', 'tuan.trainer@gym.com', 'Đà Nẵng', GETDATE(), 'ACTIVE'),
('MEMBER', 'Phạm Văn', 'Nam', 'Nam', '1998-12-10', '0901234567', 'nam.member@gmail.com', 'Hà Nội', GETDATE(), 'ACTIVE');

-- Tài khoản (sẽ được tạo bởi Identity system khi chạy ứng dụng)

-- Gói tập
INSERT INTO GoiTap (TenGoiTap, MoTa, Gia, ThoiHan, TrangThai) VALUES
('Gói Cơ Bản', 'Gói tập cơ bản với đầy đủ thiết bị', 500000, 1, 'ACTIVE'),
('Gói Tiêu Chuẩn', 'Gói tập tiêu chuẩn với PT hỗ trợ', 800000, 1, 'ACTIVE'),
('Gói VIP', 'Gói tập VIP với PT riêng và nhiều ưu đãi', 1200000, 1, 'ACTIVE');

-- Lớp học
INSERT INTO LopHoc (TenLop, MoTa, HuanLuyenVienId, ThoiGian, SucChua, Gia, TrangThai) VALUES
('Yoga Buổi Sáng', 'Lớp yoga thư giãn buổi sáng', 2, 'Thứ 2,4,6 - 07:00-08:00', 15, 200000, 'ACTIVE'),
('Gym Tăng Cơ', 'Lớp tập gym tăng cơ bắp', 3, 'Thứ 3,5,7 - 18:00-19:30', 10, 300000, 'ACTIVE'),
('Cardio Buổi Tối', 'Lớp cardio giảm cân buổi tối', 2, 'Thứ 2,4,6 - 19:00-20:00', 20, 150000, 'ACTIVE');

PRINT 'Database reset completed successfully!';
