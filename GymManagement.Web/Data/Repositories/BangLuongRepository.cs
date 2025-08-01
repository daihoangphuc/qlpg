using GymManagement.Web.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Web.Data.Repositories
{
    public class BangLuongRepository : Repository<BangLuong>, IBangLuongRepository
    {
        public BangLuongRepository(GymDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<BangLuong>> GetByHlvIdAsync(int hlvId)
        {
            return await _context.BangLuongs
                .Include(b => b.Hlv)
                .Where(b => b.HlvId == hlvId)
                .OrderByDescending(b => b.Thang)
                .ToListAsync();
        }

        public async Task<IEnumerable<BangLuong>> GetByMonthAsync(string thang)
        {
            return await _context.BangLuongs
                .Include(b => b.Hlv)
                .Where(b => b.Thang == thang)
                .OrderBy(b => b.Hlv.Ho)
                .ThenBy(b => b.Hlv.Ten)
                .ToListAsync();
        }

        public async Task<BangLuong?> GetByHlvAndMonthAsync(int hlvId, string thang)
        {
            return await _context.BangLuongs
                .Include(b => b.Hlv)
                .FirstOrDefaultAsync(b => b.HlvId == hlvId && b.Thang == thang);
        }

        public async Task<IEnumerable<BangLuong>> GetUnpaidSalariesAsync()
        {
            return await _context.BangLuongs
                .Include(b => b.Hlv)
                .Where(b => b.NgayThanhToan == null)
                .OrderBy(b => b.Thang)
                .ThenBy(b => b.Hlv.Ho)
                .ToListAsync();
        }

        public async Task<IEnumerable<BangLuong>> GetPaidSalariesAsync()
        {
            return await _context.BangLuongs
                .Include(b => b.Hlv)
                .Where(b => b.NgayThanhToan != null)
                .OrderByDescending(b => b.NgayThanhToan)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalSalaryByMonthAsync(string thang)
        {
            return await _context.BangLuongs
                .Where(b => b.Thang == thang)
                .SumAsync(b => b.LuongCoBan);
        }

        public async Task<decimal> GetTotalCommissionByMonthAsync(string thang)
        {
            return await _context.BangLuongs
                .Where(b => b.Thang == thang)
                .SumAsync(b => b.TienHoaHong);
        }

        public async Task<IEnumerable<BangLuong>> GetSalariesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.BangLuongs
                .Include(b => b.Hlv)
                .Where(b => b.NgayThanhToan >= startDate && b.NgayThanhToan <= endDate)
                .OrderByDescending(b => b.NgayThanhToan)
                .ToListAsync();
        }
    }
}
