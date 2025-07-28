-- Test validation by creating a conflicting class
-- This should trigger the validation warning

-- First, let's see current classes
SELECT 
    LopHocId,
    TenLop,
    ThuTrongTuan,
    FORMAT(GioBatDau, 'HH:mm') as GioBatDau,
    FORMAT(GioKetThuc, 'HH:mm') as GioKetThuc,
    TrangThai
FROM LopHocs 
ORDER BY LopHocId;

-- Now let's try to insert a conflicting class
-- This should be blocked by our validation
INSERT INTO LopHocs (
    TenLop, 
    ThuTrongTuan, 
    GioBatDau, 
    GioKetThuc, 
    SucChua, 
    TrangThai,
    HlvId,
    ThoiLuong,
    MoTa
) VALUES (
    'Test Conflict Class',
    'Tuesday,Thursday',  -- Same days as Gym Tang Co
    '19:15:00',          -- Overlaps with both Gym (18:00-19:30) and Cardio (19:00-20:00)
    '20:15:00',
    10,
    'OPEN',
    3,                   -- Different trainer
    60,
    'Test class to validate schedule conflicts'
);
