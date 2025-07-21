using GymManagement.Web.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Web.Data.Repositories
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(GymDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Booking>> GetByThanhVienIdAsync(int thanhVienId)
        {
            return await _context.Bookings
                .Include(b => b.ThanhVien)
                .Include(b => b.LopHoc)
                .Include(b => b.LichLop)
                .Where(b => b.ThanhVienId == thanhVienId)
                .OrderByDescending(b => b.Ngay)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetByLopHocIdAsync(int lopHocId)
        {
            return await _context.Bookings
                .Include(b => b.ThanhVien)
                .Include(b => b.LopHoc)
                .Include(b => b.LichLop)
                .Where(b => b.LopHocId == lopHocId)
                .OrderByDescending(b => b.Ngay)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetByLichLopIdAsync(int lichLopId)
        {
            return await _context.Bookings
                .Include(b => b.ThanhVien)
                .Include(b => b.LopHoc)
                .Include(b => b.LichLop)
                .Where(b => b.LichLopId == lichLopId)
                .OrderByDescending(b => b.Ngay)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByDateAsync(DateTime date)
        {
            return await _context.Bookings
                .Include(b => b.ThanhVien)
                .Include(b => b.LopHoc)
                .Include(b => b.LichLop)
                .Where(b => b.Ngay.Date == date.Date)
                .OrderBy(b => b.LichLop.GioBatDau)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetActiveBookingsAsync()
        {
            return await _context.Bookings
                .Include(b => b.ThanhVien)
                .Include(b => b.LopHoc)
                .Include(b => b.LichLop)
                .Where(b => b.TrangThai == "BOOKED" && b.Ngay >= DateTime.Today)
                .OrderBy(b => b.Ngay)
                .ThenBy(b => b.LichLop.GioBatDau)
                .ToListAsync();
        }

        public async Task<int> CountBookingsForClassAsync(int lopHocId, DateTime date)
        {
            return await _context.Bookings
                .CountAsync(b => b.LopHocId == lopHocId && 
                                b.Ngay.Date == date.Date && 
                                b.TrangThai == "BOOKED");
        }

        public async Task<int> CountBookingsForScheduleAsync(int lichLopId)
        {
            return await _context.Bookings
                .CountAsync(b => b.LichLopId == lichLopId && b.TrangThai == "BOOKED");
        }

        public async Task<bool> HasBookingAsync(int thanhVienId, int? lopHocId, int? lichLopId, DateTime date)
        {
            return await _context.Bookings
                .AnyAsync(b => b.ThanhVienId == thanhVienId &&
                              b.LopHocId == lopHocId &&
                              b.LichLopId == lichLopId &&
                              b.Ngay.Date == date.Date &&
                              b.TrangThai == "BOOKED");
        }

        public async Task<Booking?> GetActiveBookingAsync(int thanhVienId, int? lopHocId, int? lichLopId, DateTime date)
        {
            return await _context.Bookings
                .Include(b => b.ThanhVien)
                .Include(b => b.LopHoc)
                .Include(b => b.LichLop)
                .FirstOrDefaultAsync(b => b.ThanhVienId == thanhVienId &&
                                         b.LopHocId == lopHocId &&
                                         b.LichLopId == lichLopId &&
                                         b.Ngay.Date == date.Date &&
                                         b.TrangThai == "BOOKED");
        }
    }
}
