-- Script để sửa bảng CauHinhHoaHongs
-- Thêm cột NgayTao nếu chưa có

-- Kiểm tra và thêm cột NgayTao
IF NOT EXISTS (
    SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'CauHinhHoaHongs' 
    AND COLUMN_NAME = 'NgayTao'
)
BEGIN
    ALTER TABLE CauHinhHoaHongs
    ADD NgayTao DATETIME NOT NULL DEFAULT GETDATE();
    
    PRINT 'Đã thêm cột NgayTao vào bảng CauHinhHoaHongs';
END
ELSE
BEGIN
    PRINT 'Cột NgayTao đã tồn tại trong bảng CauHinhHoaHongs';
END;

-- Hiển thị cấu trúc bảng sau khi sửa
EXEC sp_columns @table_name = 'CauHinhHoaHongs';
