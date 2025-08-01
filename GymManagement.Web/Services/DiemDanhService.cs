using GymManagement.Web.Data;
using GymManagement.Web.Data.Models;
using GymManagement.Web.Data.Repositories;

namespace GymManagement.Web.Services
{
    public class DiemDanhService : IDiemDanhService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDiemDanhRepository _diemDanhRepository;
        private readonly INguoiDungRepository _nguoiDungRepository;
        private readonly IThongBaoService _thongBaoService;

        public DiemDanhService(
            IUnitOfWork unitOfWork,
            IDiemDanhRepository diemDanhRepository,
            INguoiDungRepository nguoiDungRepository,
            IThongBaoService thongBaoService)
        {
            _unitOfWork = unitOfWork;
            _diemDanhRepository = diemDanhRepository;
            _nguoiDungRepository = nguoiDungRepository;
            _thongBaoService = thongBaoService;
        }

        public async Task<IEnumerable<DiemDanh>> GetAllAsync()
        {
            return await _diemDanhRepository.GetAllAsync();
        }

        public async Task<DiemDanh?> GetByIdAsync(int id)
        {
            return await _diemDanhRepository.GetByIdAsync(id);
        }

        public async Task<DiemDanh> CreateAsync(DiemDanh diemDanh)
        {
            var created = await _diemDanhRepository.AddAsync(diemDanh);
            await _unitOfWork.SaveChangesAsync();
            return created;
        }

        public async Task<DiemDanh> UpdateAsync(DiemDanh diemDanh)
        {
            var updated = await _diemDanhRepository.UpdateAsync(diemDanh);
            await _unitOfWork.SaveChangesAsync();
            return updated;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var diemDanh = await _diemDanhRepository.GetByIdAsync(id);
            if (diemDanh == null) return false;

            await _diemDanhRepository.DeleteAsync(diemDanh);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<DiemDanh>> GetByMemberIdAsync(int thanhVienId)
        {
            return await _diemDanhRepository.GetByThanhVienIdAsync(thanhVienId);
        }

        public async Task<IEnumerable<DiemDanh>> GetTodayAttendanceAsync()
        {
            return await _diemDanhRepository.GetTodayAttendanceAsync();
        }

        public async Task<DiemDanh?> GetLatestAttendanceAsync(int thanhVienId)
        {
            return await _diemDanhRepository.GetLatestAttendanceAsync(thanhVienId);
        }

        public async Task<bool> CheckInAsync(int thanhVienId, string? anhMinhChung = null)
        {
            // Check if member exists and is active
            var thanhVien = await _nguoiDungRepository.GetByIdAsync(thanhVienId);
            if (thanhVien == null || thanhVien.TrangThai != "ACTIVE" || thanhVien.LoaiNguoiDung != "THANHVIEN")
                return false;

            // Check if already checked in today
            if (await _diemDanhRepository.HasAttendanceToday(thanhVienId))
                return false;

            // Create attendance record
            var diemDanh = new DiemDanh
            {
                ThanhVienId = thanhVienId,
                ThoiGian = DateTime.Now,
                KetQuaNhanDang = true, // Manual check-in is always successful
                AnhMinhChung = anhMinhChung
            };

            await _diemDanhRepository.AddAsync(diemDanh);
            await _unitOfWork.SaveChangesAsync();

            // Create workout session record
            var buoiTap = new BuoiTap
            {
                ThanhVienId = thanhVienId,
                ThoiGianVao = DateTime.Now,
                GhiChu = "Check-in thủ công"
            };

            await _unitOfWork.Context.BuoiTaps.AddAsync(buoiTap);
            await _unitOfWork.SaveChangesAsync();

            // Send notification
            await _thongBaoService.CreateNotificationAsync(
                thanhVienId,
                "Check-in thành công",
                $"Bạn đã check-in thành công lúc {DateTime.Now:HH:mm dd/MM/yyyy}",
                "APP"
            );

            return true;
        }

        public async Task<bool> CheckInWithFaceRecognitionAsync(int thanhVienId, byte[] faceImage)
        {
            // Check if member exists and is active
            var thanhVien = await _nguoiDungRepository.GetByIdAsync(thanhVienId);
            if (thanhVien == null || thanhVien.TrangThai != "ACTIVE" || thanhVien.LoaiNguoiDung != "THANHVIEN")
                return false;

            // Check if already checked in today
            if (await _diemDanhRepository.HasAttendanceToday(thanhVienId))
                return false;

            // TODO: Implement face recognition logic
            // For now, we'll simulate face recognition
            bool faceRecognitionResult = await SimulateFaceRecognition(thanhVienId, faceImage);

            // Create attendance record
            var diemDanh = new DiemDanh
            {
                ThanhVienId = thanhVienId,
                ThoiGian = DateTime.Now,
                KetQuaNhanDang = faceRecognitionResult,
                AnhMinhChung = $"face_recognition_{DateTime.Now:yyyyMMddHHmmss}.jpg"
            };

            await _diemDanhRepository.AddAsync(diemDanh);

            if (faceRecognitionResult)
            {
                // Create workout session record
                var buoiTap = new BuoiTap
                {
                    ThanhVienId = thanhVienId,
                    ThoiGianVao = DateTime.Now,
                    GhiChu = "Check-in bằng nhận diện khuôn mặt"
                };

                await _unitOfWork.Context.BuoiTaps.AddAsync(buoiTap);

                // Send success notification
                await _thongBaoService.CreateNotificationAsync(
                    thanhVienId,
                    "Check-in thành công",
                    $"Bạn đã check-in thành công bằng nhận diện khuôn mặt lúc {DateTime.Now:HH:mm dd/MM/yyyy}",
                    "APP"
                );
            }
            else
            {
                // Send failure notification
                await _thongBaoService.CreateNotificationAsync(
                    thanhVienId,
                    "Check-in thất bại",
                    "Không thể nhận diện khuôn mặt. Vui lòng thử lại hoặc liên hệ nhân viên.",
                    "APP"
                );
            }

            await _unitOfWork.SaveChangesAsync();
            return faceRecognitionResult;
        }

        public async Task<bool> HasCheckedInTodayAsync(int thanhVienId)
        {
            return await _diemDanhRepository.HasAttendanceToday(thanhVienId);
        }

        public async Task<int> GetTodayAttendanceCountAsync()
        {
            return await _diemDanhRepository.CountAttendanceByDateAsync(DateTime.Today);
        }

        public async Task<int> GetMemberAttendanceCountAsync(int thanhVienId, DateTime startDate, DateTime endDate)
        {
            return await _diemDanhRepository.CountAttendanceByMemberAsync(thanhVienId, startDate, endDate);
        }

        public async Task<IEnumerable<DiemDanh>> GetAttendanceReportAsync(DateTime startDate, DateTime endDate)
        {
            return await _diemDanhRepository.GetByDateRangeAsync(startDate, endDate);
        }

        private async Task<bool> SimulateFaceRecognition(int thanhVienId, byte[] faceImage)
        {
            // This is a placeholder for actual face recognition implementation
            // In a real system, you would:
            // 1. Load the member's face template from MauMat table
            // 2. Compare it with the provided face image
            // 3. Return true if similarity is above threshold

            await Task.Delay(1000); // Simulate processing time
            
            // For demo purposes, return true 90% of the time
            var random = new Random();
            return random.NextDouble() > 0.1;
        }
    }
}
