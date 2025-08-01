using GymManagement.Web.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Web.Data.Repositories
{
    public class DiemDanhRepository : Repository<DiemDanh>, IDiemDanhRepository
    {
        public DiemDanhRepository(GymDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DiemDanh>> GetByThanhVienIdAsync(int thanhVienId)
        {
            return await _context.DiemDanhs
                .Include(d => d.ThanhVien)
                .Where(d => d.ThanhVienId == thanhVienId)
                .OrderByDescending(d => d.ThoiGian)
                .ToListAsync();
        }

        public async Task<IEnumerable<DiemDanh>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.DiemDanhs
                .Include(d => d.ThanhVien)
                .Where(d => d.ThoiGian >= startDate && d.ThoiGian <= endDate)
                .OrderByDescending(d => d.ThoiGian)
                .ToListAsync();
        }

        public async Task<IEnumerable<DiemDanh>> GetTodayAttendanceAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            
            return await _context.DiemDanhs
                .Include(d => d.ThanhVien)
                .Where(d => d.ThoiGian >= today && d.ThoiGian < tomorrow)
                .OrderByDescending(d => d.ThoiGian)
                .ToListAsync();
        }

        public async Task<DiemDanh?> GetLatestAttendanceAsync(int thanhVienId)
        {
            return await _context.DiemDanhs
                .Include(d => d.ThanhVien)
                .Where(d => d.ThanhVienId == thanhVienId)
                .OrderByDescending(d => d.ThoiGian)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasAttendanceToday(int thanhVienId)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            
            return await _context.DiemDanhs
                .AnyAsync(d => d.ThanhVienId == thanhVienId && 
                              d.ThoiGian >= today && 
                              d.ThoiGian < tomorrow);
        }

        public async Task<int> CountAttendanceByDateAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);
            
            return await _context.DiemDanhs
                .CountAsync(d => d.ThoiGian >= startDate && d.ThoiGian < endDate);
        }

        public async Task<int> CountAttendanceByMemberAsync(int thanhVienId, DateTime startDate, DateTime endDate)
        {
            return await _context.DiemDanhs
                .CountAsync(d => d.ThanhVienId == thanhVienId && 
                                d.ThoiGian >= startDate && 
                                d.ThoiGian <= endDate);
        }

        public async Task<IEnumerable<DiemDanh>> GetSuccessfulAttendanceAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.DiemDanhs
                .Include(d => d.ThanhVien)
                .Where(d => d.KetQuaNhanDang == true && 
                           d.ThoiGian >= startDate && 
                           d.ThoiGian <= endDate)
                .OrderByDescending(d => d.ThoiGian)
                .ToListAsync();
        }
    }
}
