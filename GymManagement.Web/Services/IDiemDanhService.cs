using GymManagement.Web.Data.Models;

namespace GymManagement.Web.Services
{
    public interface IDiemDanhService
    {
        Task<IEnumerable<DiemDanh>> GetAllAsync();
        Task<DiemDanh?> GetByIdAsync(int id);
        Task<DiemDanh> CreateAsync(DiemDanh diemDanh);
        Task<DiemDanh> UpdateAsync(DiemDanh diemDanh);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<DiemDanh>> GetByMemberIdAsync(int thanhVienId);
        Task<IEnumerable<DiemDanh>> GetTodayAttendanceAsync();
        Task<DiemDanh?> GetLatestAttendanceAsync(int thanhVienId);
        Task<bool> CheckInAsync(int thanhVienId, string? anhMinhChung = null);
        Task<bool> CheckInWithFaceRecognitionAsync(int thanhVienId, byte[] faceImage);
        Task<bool> HasCheckedInTodayAsync(int thanhVienId);
        Task<int> GetTodayAttendanceCountAsync();
        Task<int> GetMemberAttendanceCountAsync(int thanhVienId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<DiemDanh>> GetAttendanceReportAsync(DateTime startDate, DateTime endDate);
    }
}
