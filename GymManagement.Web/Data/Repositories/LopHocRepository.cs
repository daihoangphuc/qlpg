using GymManagement.Web.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Web.Data.Repositories
{
    public class LopHocRepository : Repository<LopHoc>, ILopHocRepository
    {
        public LopHocRepository(GymDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<LopHoc>> GetActiveClassesAsync()
        {
            return await _dbSet
                .Where(x => x.TrangThai == "OPEN")
                .Include(x => x.Hlv)
                .ToListAsync();
        }

        public async Task<IEnumerable<LopHoc>> GetByHuanLuyenVienAsync(int hlvId)
        {
            return await _dbSet
                .Where(x => x.HlvId == hlvId)
                .Include(x => x.Hlv)
                .ToListAsync();
        }

        public async Task<IEnumerable<LopHoc>> GetByThuTrongTuanAsync(string thuTrongTuan)
        {
            return await _dbSet
                .Where(x => x.ThuTrongTuan.Contains(thuTrongTuan))
                .Include(x => x.Hlv)
                .ToListAsync();
        }

        public async Task<IEnumerable<LopHoc>> GetAvailableClassesAsync(DateOnly date)
        {
            var dayOfWeek = date.DayOfWeek.ToString().ToUpper();
            
            return await _dbSet
                .Where(x => x.TrangThai == "OPEN" && x.ThuTrongTuan.Contains(dayOfWeek))
                .Include(x => x.Hlv)
                .ToListAsync();
        }

        public async Task<LopHoc?> GetWithLichLopsAsync(int lopHocId)
        {
            return await _dbSet
                .Include(x => x.LichLops)
                .Include(x => x.Hlv)
                .FirstOrDefaultAsync(x => x.LopHocId == lopHocId);
        }

        public async Task<IEnumerable<LopHoc>> GetClassesWithAvailableSlotsAsync()
        {
            return await _dbSet
                .Where(x => x.TrangThai == "OPEN")
                .Include(x => x.Hlv)
                .Include(x => x.DangKys)
                .Where(x => x.DangKys.Count(d => d.TrangThai == "ACTIVE") < x.SucChua)
                .ToListAsync();
        }

        public async Task<IEnumerable<LopHoc>> GetClassesByTrainerAsync(int trainerId)
        {
            return await _dbSet
                .Where(x => x.HlvId == trainerId)
                .Include(x => x.Hlv)
                .ToListAsync();
        }
    }
}
