using GymManagement.Web.Data.Models;

namespace GymManagement.Web.Services
{
    public interface IBookingService
    {
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(int id);
        Task<Booking> CreateAsync(Booking booking);
        Task<Booking> UpdateAsync(Booking booking);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Booking>> GetByMemberIdAsync(int thanhVienId);
        Task<IEnumerable<Booking>> GetByClassIdAsync(int lopHocId);
        Task<IEnumerable<Booking>> GetTodayBookingsAsync();
        Task<bool> BookClassAsync(int thanhVienId, int lopHocId, DateTime date);
        Task<bool> BookScheduleAsync(int thanhVienId, int lichLopId);
        Task<bool> CancelBookingAsync(int bookingId);
        Task<bool> CanBookAsync(int thanhVienId, int lopHocId, DateTime date);
        Task<int> GetAvailableSlotsAsync(int lopHocId, DateTime date);
        Task<IEnumerable<Booking>> GetUpcomingBookingsAsync(int thanhVienId);
    }
}
