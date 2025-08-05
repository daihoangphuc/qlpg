USE QLPG_Remote;
GO

-- Xóa bảng CauHinhHoaHong cũ nếu có
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CauHinhHoaHong]') AND type in (N'U'))
BEGIN
    DROP TABLE CauHinhHoaHong;
    PRINT 'Đã xóa bảng CauHinhHoaHong cũ';
END
GO

-- Tạo lại bảng CauHinhHoaHong đơn giản hơn
CREATE TABLE CauHinhHoaHong(
    CauHinhHoaHongId INT IDENTITY(1,1) PRIMARY KEY,
    GoiTapId         INT NULL,
    PhanTramHoaHong  DECIMAL(5,2) NOT NULL CHECK (PhanTramHoaHong >= 0 AND PhanTramHoaHong <= 100),
    NgayTao          DATETIME DEFAULT GETDATE()
);

PRINT 'Đã tạo bảng CauHinhHoaHong mới';

-- Thêm dữ liệu mẫu
INSERT INTO CauHinhHoaHong (GoiTapId, PhanTramHoaHong) 
VALUES 
    (NULL, 10), -- Hoa hồng mặc định 10%
    (1, 15),    -- Hoa hồng cho gói tập 1: 15%
    (2, 12);    -- Hoa hồng cho gói tập 2: 12%

PRINT 'Đã thêm dữ liệu mẫu cho CauHinhHoaHong';

-- Kiểm tra lại cấu trúc database
SELECT 
    t.name AS TableName,
    COUNT(c.column_id) AS ColumnCount
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
WHERE t.name IN ('BangLuong', 'CauHinhHoaHong')
GROUP BY t.name;

PRINT '';
PRINT 'Database đã sẵn sàng cho chức năng lương!';
