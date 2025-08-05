-- Kiểm tra và thêm cột NgayTao nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BangLuong]') AND name = 'NgayTao')
BEGIN
    ALTER TABLE BangLuong ADD NgayTao DATETIME2 DEFAULT SYSDATETIME();
END
GO

-- Kiểm tra và thêm cột GhiChu nếu chưa có  
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BangLuong]') AND name = 'GhiChu')
BEGIN
    ALTER TABLE BangLuong ADD GhiChu NVARCHAR(500);
END
GO

-- Update giá trị NgayTao cho các bản ghi hiện có (nếu có)
UPDATE BangLuong SET NgayTao = SYSDATETIME() WHERE NgayTao IS NULL;
GO

PRINT 'Đã cập nhật cấu trúc bảng BangLuong thành công!';
