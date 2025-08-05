-- Kiểm tra database hiện tại
USE QLPG;  -- Hoặc tên database của bạn
GO

-- Kiểm tra xem bảng BangLuong đã tồn tại chưa
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BangLuong]') AND type in (N'U'))
BEGIN
    PRINT 'Bảng BangLuong chưa tồn tại. Đang tạo bảng...';
    
    -- Tạo bảng BangLuong
    CREATE TABLE BangLuong(
        BangLuongId   INT IDENTITY PRIMARY KEY,
        HlvId         INT REFERENCES NguoiDung(NguoiDungId),
        Thang         NVARCHAR(7) NOT NULL, -- 'YYYY-MM'
        LuongCoBan    DECIMAL(12,2) NOT NULL,
        TienHoaHong   DECIMAL(12,2) DEFAULT 0,
        NgayThanhToan DATE,
        NgayTao       DATETIME DEFAULT GETDATE(),
        GhiChu        NVARCHAR(500)
    );
    
    PRINT 'Đã tạo bảng BangLuong thành công!';
END
ELSE
BEGIN
    PRINT 'Bảng BangLuong đã tồn tại.';
    
    -- Kiểm tra và thêm cột NgayTao nếu chưa có
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BangLuong]') AND name = 'NgayTao')
    BEGIN
        ALTER TABLE BangLuong ADD NgayTao DATETIME DEFAULT GETDATE();
        PRINT 'Đã thêm cột NgayTao';
    END

    -- Kiểm tra và thêm cột GhiChu nếu chưa có  
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BangLuong]') AND name = 'GhiChu')
    BEGIN
        ALTER TABLE BangLuong ADD GhiChu NVARCHAR(500);
        PRINT 'Đã thêm cột GhiChu';
    END
END
GO

-- Kiểm tra lại cấu trúc bảng
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length,
    c.is_nullable,
    c.is_identity
FROM sys.columns c
INNER JOIN sys.types t ON c.system_type_id = t.system_type_id
WHERE c.object_id = OBJECT_ID('BangLuong')
ORDER BY c.column_id;
GO

PRINT 'Hoàn thành kiểm tra bảng BangLuong!';
