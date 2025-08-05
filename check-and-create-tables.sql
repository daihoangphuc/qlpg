-- Script kiểm tra và tạo các bảng cần thiết cho chức năng lương
-- Chạy script này trước khi demo

-- 1. Kiểm tra database
IF DB_ID('QLPG') IS NULL
BEGIN
    PRINT 'Database QLPG không tồn tại. Vui lòng tạo database trước.';
    RETURN;
END
GO

USE QLPG;
GO

-- 2. Kiểm tra bảng NguoiDung (bắt buộc phải có trước)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NguoiDung]') AND type in (N'U'))
BEGIN
    PRINT 'CẢNH BÁO: Bảng NguoiDung chưa tồn tại! Vui lòng chạy script CSDL.SQL đầy đủ trước.';
    RETURN;
END
GO

-- 3. Tạo bảng BangLuong nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BangLuong]') AND type in (N'U'))
BEGIN
    PRINT 'Đang tạo bảng BangLuong...';
    
    CREATE TABLE BangLuong(
        BangLuongId   INT IDENTITY(1,1) PRIMARY KEY,
        HlvId         INT NULL,
        Thang         NVARCHAR(7) NOT NULL,
        LuongCoBan    DECIMAL(12,2) NOT NULL,
        TienHoaHong   DECIMAL(12,2) DEFAULT 0,
        NgayThanhToan DATE NULL,
        NgayTao       DATETIME DEFAULT GETDATE(),
        GhiChu        NVARCHAR(500) NULL,
        CONSTRAINT FK_BangLuong_NguoiDung FOREIGN KEY (HlvId) REFERENCES NguoiDung(NguoiDungId)
    );
    
    PRINT '✓ Đã tạo bảng BangLuong';
END
ELSE
BEGIN
    PRINT '✓ Bảng BangLuong đã tồn tại';
END
GO

-- 4. Tạo bảng CauHinhHoaHong nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CauHinhHoaHong]') AND type in (N'U'))
BEGIN
    PRINT 'Đang tạo bảng CauHinhHoaHong...';
    
    CREATE TABLE CauHinhHoaHong(
        CauHinhHoaHongId INT IDENTITY(1,1) PRIMARY KEY,
        GoiTapId         INT NULL,
        LopHocId         INT NULL,
        PhanTramHoaHong  DECIMAL(5,2) NOT NULL CHECK (PhanTramHoaHong >= 0 AND PhanTramHoaHong <= 100),
        KichHoat         BIT DEFAULT 1,
        NgayTao          DATETIME DEFAULT GETDATE()
    );
    
    -- Thêm dữ liệu mẫu cho cấu hình hoa hồng
    INSERT INTO CauHinhHoaHong (GoiTapId, LopHocId, PhanTramHoaHong, KichHoat) 
    VALUES 
        (NULL, NULL, 10, 1), -- Hoa hồng mặc định 10%
        (1, NULL, 15, 1),    -- Hoa hồng cho gói tập 1: 15%
        (2, NULL, 12, 1);    -- Hoa hồng cho gói tập 2: 12%
    
    PRINT '✓ Đã tạo bảng CauHinhHoaHong và thêm dữ liệu mẫu';
END
ELSE
BEGIN
    PRINT '✓ Bảng CauHinhHoaHong đã tồn tại';
END
GO

-- 5. Kiểm tra và thêm cột còn thiếu trong BangLuong
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BangLuong]') AND type in (N'U'))
BEGIN
    -- Thêm cột NgayTao nếu chưa có
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BangLuong]') AND name = 'NgayTao')
    BEGIN
        ALTER TABLE BangLuong ADD NgayTao DATETIME DEFAULT GETDATE();
        PRINT '✓ Đã thêm cột NgayTao vào BangLuong';
    END

    -- Thêm cột GhiChu nếu chưa có
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BangLuong]') AND name = 'GhiChu')
    BEGIN
        ALTER TABLE BangLuong ADD GhiChu NVARCHAR(500) NULL;
        PRINT '✓ Đã thêm cột GhiChu vào BangLuong';
    END
END
GO

-- 6. Tạo index cho hiệu năng
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BangLuong_HlvId_Thang')
BEGIN
    CREATE INDEX IX_BangLuong_HlvId_Thang ON BangLuong(HlvId, Thang);
    PRINT '✓ Đã tạo index IX_BangLuong_HlvId_Thang';
END
GO

-- 7. Hiển thị cấu trúc bảng BangLuong
PRINT '';
PRINT '=== CẤU TRÚC BẢNG BANGLUONG ===';
SELECT 
    c.name AS [Tên cột],
    t.name AS [Kiểu dữ liệu],
    CASE 
        WHEN t.name IN ('nvarchar', 'varchar', 'char', 'nchar') THEN CAST(c.max_length/2 AS VARCHAR) 
        WHEN t.name IN ('decimal', 'numeric') THEN CAST(c.precision AS VARCHAR) + ',' + CAST(c.scale AS VARCHAR)
        ELSE ''
    END AS [Kích thước],
    CASE c.is_nullable WHEN 1 THEN 'YES' ELSE 'NO' END AS [Cho phép NULL],
    CASE c.is_identity WHEN 1 THEN 'YES' ELSE 'NO' END AS [Identity]
FROM sys.columns c
INNER JOIN sys.types t ON c.system_type_id = t.system_type_id AND t.user_type_id = t.system_type_id
WHERE c.object_id = OBJECT_ID('BangLuong')
ORDER BY c.column_id;

PRINT '';
PRINT '=== HOÀN THÀNH KIỂM TRA DATABASE ===';
PRINT 'Bạn có thể chạy ứng dụng và demo chức năng lương!';
