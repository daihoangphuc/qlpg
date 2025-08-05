-- Script tổng hợp để fix toàn bộ database cho chức năng quản lý lương
-- Chạy script này để đảm bảo database hoạt động đúng

USE QLPG_Remote;
GO

-- ===========================
-- 1. Tạo bảng BangLuongs nếu chưa có
-- ===========================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BangLuongs')
BEGIN
    CREATE TABLE BangLuongs (
        BangLuongId INT IDENTITY(1,1) PRIMARY KEY,
        HlvId INT NULL,
        Thang NVARCHAR(7) NOT NULL,
        LuongCoBan DECIMAL(12,2) NOT NULL,
        TienHoaHong DECIMAL(12,2) NOT NULL DEFAULT 0,
        NgayThanhToan DATE NULL,
        GhiChu NVARCHAR(500) NULL,
        NgayTao DATETIME NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (HlvId) REFERENCES NguoiDungs(NguoiDungId)
    );
    
    -- Tạo index cho hiệu suất
    CREATE INDEX IX_BangLuongs_HlvId ON BangLuongs(HlvId);
    CREATE INDEX IX_BangLuongs_Thang ON BangLuongs(Thang);
    CREATE INDEX IX_BangLuongs_NgayThanhToan ON BangLuongs(NgayThanhToan);
    
    PRINT 'Đã tạo bảng BangLuongs';
END
ELSE
BEGIN
    -- Kiểm tra và thêm các cột còn thiếu
    
    -- Thêm cột NgayTao nếu chưa có
    IF NOT EXISTS (
        SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'BangLuongs' AND COLUMN_NAME = 'NgayTao'
    )
    BEGIN
        ALTER TABLE BangLuongs ADD NgayTao DATETIME NOT NULL DEFAULT GETDATE();
        PRINT 'Đã thêm cột NgayTao vào bảng BangLuongs';
    END
    
    -- Thêm cột GhiChu nếu chưa có
    IF NOT EXISTS (
        SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'BangLuongs' AND COLUMN_NAME = 'GhiChu'
    )
    BEGIN
        ALTER TABLE BangLuongs ADD GhiChu NVARCHAR(500) NULL;
        PRINT 'Đã thêm cột GhiChu vào bảng BangLuongs';
    END
END;
GO

-- ===========================
-- 2. Tạo hoặc sửa bảng CauHinhHoaHongs
-- ===========================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CauHinhHoaHongs')
BEGIN
    -- Kiểm tra và thêm cột NgayTao
    IF NOT EXISTS (
        SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'CauHinhHoaHongs' AND COLUMN_NAME = 'NgayTao'
    )
    BEGIN
        ALTER TABLE CauHinhHoaHongs ADD NgayTao DATETIME NOT NULL DEFAULT GETDATE();
        PRINT 'Đã thêm cột NgayTao vào bảng CauHinhHoaHongs';
    END
END
ELSE
BEGIN
    -- Tạo bảng CauHinhHoaHongs
    CREATE TABLE CauHinhHoaHongs (
        CauHinhHoaHongId INT IDENTITY(1,1) PRIMARY KEY,
        GoiTapId INT NULL,
        PhanTramHoaHong DECIMAL(5,2) NOT NULL,
        NgayTao DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT CK_PhanTramHoaHong CHECK (PhanTramHoaHong >= 0 AND PhanTramHoaHong <= 100),
        FOREIGN KEY (GoiTapId) REFERENCES GoiTaps(GoiTapId)
    );
    
    -- Tạo index
    CREATE INDEX IX_CauHinhHoaHongs_GoiTapId ON CauHinhHoaHongs(GoiTapId);
    
    PRINT 'Đã tạo bảng CauHinhHoaHongs';
END;
GO

-- ===========================
-- 3. Thêm dữ liệu mẫu cho CauHinhHoaHong nếu chưa có
-- ===========================
IF NOT EXISTS (SELECT * FROM CauHinhHoaHongs)
BEGIN
    -- Lấy danh sách GoiTapId từ bảng GoiTaps
    INSERT INTO CauHinhHoaHongs (GoiTapId, PhanTramHoaHong, NgayTao)
    SELECT 
        GoiTapId,
        CASE 
            WHEN Gia >= 5000000 THEN 15  -- Gói VIP: 15%
            WHEN Gia >= 2000000 THEN 10  -- Gói trung bình: 10%
            ELSE 5                        -- Gói cơ bản: 5%
        END,
        GETDATE()
    FROM GoiTaps
    WHERE TrangThai = 'ACTIVE';
    
    -- Thêm cấu hình chung (không có GoiTapId cụ thể)
    INSERT INTO CauHinhHoaHongs (GoiTapId, PhanTramHoaHong, NgayTao)
    VALUES (NULL, 5, GETDATE());
    
    PRINT 'Đã thêm dữ liệu mẫu cho bảng CauHinhHoaHongs';
END;
GO

-- ===========================
-- 4. Kiểm tra và sửa các cột trong bảng khác nếu cần
-- ===========================

-- Kiểm tra bảng NguoiDungs có cột Email không
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'NguoiDungs' AND COLUMN_NAME = 'Email'
)
BEGIN
    ALTER TABLE NguoiDungs ADD Email NVARCHAR(256) NULL;
    PRINT 'Đã thêm cột Email vào bảng NguoiDungs';
END;
GO

-- ===========================
-- 5. Hiển thị kết quả kiểm tra
-- ===========================
PRINT '';
PRINT '=== KẾT QUẢ KIỂM TRA DATABASE ===';
PRINT '';

-- Kiểm tra bảng BangLuongs
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'BangLuongs')
BEGIN
    PRINT '✓ Bảng BangLuongs: OK';
    DECLARE @bangluong_count INT = (SELECT COUNT(*) FROM BangLuongs);
    PRINT '  - Số bản ghi: ' + CAST(@bangluong_count AS NVARCHAR(10));
END
ELSE
BEGIN
    PRINT '✗ Bảng BangLuongs: KHÔNG TỒN TẠI';
END;

-- Kiểm tra bảng CauHinhHoaHongs
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CauHinhHoaHongs')
BEGIN
    PRINT '✓ Bảng CauHinhHoaHongs: OK';
    DECLARE @cauhinh_count INT = (SELECT COUNT(*) FROM CauHinhHoaHongs);
    PRINT '  - Số bản ghi: ' + CAST(@cauhinh_count AS NVARCHAR(10));
END
ELSE
BEGIN
    PRINT '✗ Bảng CauHinhHoaHongs: KHÔNG TỒN TẠI';
END;

-- Kiểm tra HLV
DECLARE @hlv_count INT = (SELECT COUNT(*) FROM NguoiDungs WHERE LoaiNguoiDung = 'HLV' AND TrangThai = 'ACTIVE');
PRINT '';
PRINT 'Số lượng HLV đang hoạt động: ' + CAST(@hlv_count AS NVARCHAR(10));

PRINT '';
PRINT '=== HOÀN THÀNH ===';
PRINT 'Database đã sẵn sàng cho chức năng quản lý lương!';
GO
