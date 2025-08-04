using GymManagement.Web.Data;
using GymManagement.Web.Data.Models;
using GymManagement.Web.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Web.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBookingRepository _bookingRepository;
        private readonly ILopHocRepository _lopHocRepository;
        private readonly IThongBaoService _thongBaoService;

        public BookingService(
            IUnitOfWork unitOfWork,
            IBookingRepository bookingRepository,
            ILopHocRepository lopHocRepository,
            IThongBaoService thongBaoService)
        {
            _unitOfWork = unitOfWork;
            _bookingRepository = bookingRepository;
            _lopHocRepository = lopHocRepository;
            _thongBaoService = thongBaoService;
        }

        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _bookingRepository.GetAllAsync();
        }

        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _bookingRepository.GetByIdAsync(id);
        }

        public async Task<Booking> CreateAsync(Booking booking)
        {
            var created = await _bookingRepository.AddAsync(booking);
            await _unitOfWork.SaveChangesAsync();
            return created;
        }

        public async Task<Booking> UpdateAsync(Booking booking)
        {
            await _bookingRepository.UpdateAsync(booking);
            await _unitOfWork.SaveChangesAsync();
            return booking;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) return false;

            await _bookingRepository.DeleteAsync(booking);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Booking>> GetByMemberIdAsync(int thanhVienId)
        {
            return await _bookingRepository.GetByThanhVienIdAsync(thanhVienId);
        }

        public async Task<IEnumerable<Booking>> GetByClassIdAsync(int lopHocId)
        {
            return await _bookingRepository.GetByLopHocIdAsync(lopHocId);
        }

        public async Task<IEnumerable<Booking>> GetTodayBookingsAsync()
        {
            return await _bookingRepository.GetBookingsByDateAsync(DateTime.Today);
        }

        public async Task<bool> BookClassAsync(int thanhVienId, int lopHocId, DateTime date, string? ghiChu = null)
        {
            // Check if date is in the future
            if (date.Date < DateTime.Today)
                return false;

            // Check if class exists and is open
            var lopHoc = await _lopHocRepository.GetByIdAsync(lopHocId);
            if (lopHoc == null || lopHoc.TrangThai != "OPEN")
                return false;

            // Check if member already has a booking for this class on this date
            if (await _bookingRepository.HasBookingAsync(thanhVienId, lopHocId, null, date))
                return false;

            // Check if class has available slots
            var bookingCount = await _bookingRepository.CountBookingsForClassAsync(lopHocId, date);
            if (bookingCount >= lopHoc.SucChua)
                return false;

            // Create booking
            var booking = new Booking
            {
                ThanhVienId = thanhVienId,
                LopHocId = lopHocId,
                Ngay = DateOnly.FromDateTime(date),
                NgayDat = DateOnly.FromDateTime(DateTime.Now),
                TrangThai = "BOOKED",
                GhiChu = ghiChu
            };

            await _bookingRepository.AddAsync(booking);
            await _unitOfWork.SaveChangesAsync();

            // Send notification
            var thanhVien = await _unitOfWork.Context.NguoiDungs.FindAsync(thanhVienId);
            if (thanhVien != null)
            {
                await _thongBaoService.CreateNotificationAsync(
                    thanhVienId,
                    "Đặt lịch thành công",
                    $"Bạn đã đặt lịch thành công lớp {lopHoc.TenLop} vào ngày {date:dd/MM/yyyy}",
                    "APP"
                );
            }

            return true;
        }

        public async Task<bool> BookScheduleAsync(int thanhVienId, int lichLopId)
        {
            // Check if schedule exists
            var lichLop = await _unitOfWork.Context.LichLops
                .Include(l => l.LopHoc)
                .FirstOrDefaultAsync(l => l.LichLopId == lichLopId);

            if (lichLop == null || lichLop.TrangThai != "SCHEDULED")
                return false;

            // Check if member already has a booking for this schedule
            if (await _bookingRepository.HasBookingAsync(thanhVienId, lichLop.LopHocId, lichLopId, lichLop.Ngay.ToDateTime(TimeOnly.MinValue)))
                return false;

            // Check if class has available slots
            var bookingCount = await _bookingRepository.CountBookingsForScheduleAsync(lichLopId);
            if (bookingCount >= lichLop.LopHoc.SucChua)
                return false;

            // Create booking
            var booking = new Booking
            {
                ThanhVienId = thanhVienId,
                LopHocId = lichLop.LopHocId,
                LichLopId = lichLopId,
                Ngay = lichLop.Ngay,
                TrangThai = "BOOKED"
            };

            await _bookingRepository.AddAsync(booking);
            await _unitOfWork.SaveChangesAsync();

            // Send notification
            var thanhVien = await _unitOfWork.Context.NguoiDungs.FindAsync(thanhVienId);
            if (thanhVien != null)
            {
                await _thongBaoService.CreateNotificationAsync(
                    thanhVienId,
                    "Đặt lịch thành công",
                    $"Bạn đã đặt lịch thành công lớp {lichLop.LopHoc.TenLop} vào ngày {lichLop.Ngay:dd/MM/yyyy}",
                    "APP"
                );
            }

            return true;
        }

        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null || booking.TrangThai != "BOOKED")
                return false;

            // Check if cancellation is at least 2 hours before class time
            if (booking.LopHoc != null)
            {
                var classDateTime = booking.Ngay.ToDateTime(booking.LopHoc.GioBatDau);
                var hoursUntilClass = (classDateTime - DateTime.Now).TotalHours;
                
                if (hoursUntilClass < 2)
                    return false; // Cannot cancel less than 2 hours before class
            }

            booking.TrangThai = "CANCELED";
            await _unitOfWork.SaveChangesAsync();

            // Send notification
            if (booking.ThanhVienId.HasValue)
            {
                var lopHoc = await _lopHocRepository.GetByIdAsync(booking.LopHocId ?? 0);
                
                await _thongBaoService.CreateNotificationAsync(
                    booking.ThanhVienId.Value,
                    "Huỷ đặt lịch thành công",
                    $"Bạn đã huỷ đặt lịch lớp {lopHoc?.TenLop} vào ngày {booking.Ngay:dd/MM/yyyy}",
                    "APP"
                );
            }

            return true;
        }

        public async Task<bool> CanBookAsync(int thanhVienId, int lopHocId, DateTime date)
        {
            // Check if class exists and is open
            var lopHoc = await _lopHocRepository.GetByIdAsync(lopHocId);
            if (lopHoc == null || lopHoc.TrangThai != "OPEN")
                return false;

            // Check if member already has a booking for this class on this date
            if (await _bookingRepository.HasBookingAsync(thanhVienId, lopHocId, null, date))
                return false;

            // Check if class has available slots
            var bookingCount = await _bookingRepository.CountBookingsForClassAsync(lopHocId, date);
            return bookingCount < lopHoc.SucChua;
        }

        public async Task<int> GetAvailableSlotsAsync(int lopHocId, DateTime date)
        {
            var lopHoc = await _lopHocRepository.GetByIdAsync(lopHocId);
            if (lopHoc == null) return 0;

            var bookingCount = await _bookingRepository.CountBookingsForClassAsync(lopHocId, date);
            return Math.Max(0, lopHoc.SucChua - bookingCount);
        }

        public async Task<IEnumerable<Booking>> GetUpcomingBookingsAsync(int thanhVienId)
        {
            var bookings = await _bookingRepository.GetByThanhVienIdAsync(thanhVienId);
            return bookings.Where(b => b.TrangThai == "BOOKED" && b.Ngay >= DateOnly.FromDateTime(DateTime.Today));
        }
    }
}
