using GymManagement.Web.Data;
using GymManagement.Web.Data.Models;
using GymManagement.Web.Data.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Web.Services
{
    public class LopHocService : ILopHocService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILopHocRepository _lopHocRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IMemoryCache _cache;

        public LopHocService(
            IUnitOfWork unitOfWork,
            ILopHocRepository lopHocRepository,
            IBookingRepository bookingRepository,
            IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _lopHocRepository = lopHocRepository;
            _bookingRepository = bookingRepository;
            _cache = cache;
        }

        public async Task<IEnumerable<LopHoc>> GetAllAsync()
        {
            const string cacheKey = "all_classes";
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<LopHoc>? classes))
            {
                classes = await _lopHocRepository.GetAllAsync();
                _cache.Set(cacheKey, classes, TimeSpan.FromMinutes(10));
            }
            return classes ?? new List<LopHoc>();
        }

        public async Task<LopHoc?> GetByIdAsync(int id)
        {
            return await _lopHocRepository.GetByIdAsync(id);
        }

        public async Task<LopHoc> CreateAsync(LopHoc lopHoc)
        {
            var created = await _lopHocRepository.AddAsync(lopHoc);
            await _unitOfWork.SaveChangesAsync();
            
            // Clear cache
            _cache.Remove("all_classes");
            _cache.Remove("active_classes");
            
            return created;
        }

        public async Task<LopHoc> UpdateAsync(LopHoc lopHoc)
        {
            await _lopHocRepository.UpdateAsync(lopHoc);
            await _unitOfWork.SaveChangesAsync();

            // Clear cache
            _cache.Remove("all_classes");
            _cache.Remove("active_classes");

            return lopHoc;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var lopHoc = await _lopHocRepository.GetByIdAsync(id);
            if (lopHoc == null) return false;

            await _lopHocRepository.DeleteAsync(lopHoc);
            await _unitOfWork.SaveChangesAsync();
            
            // Clear cache
            _cache.Remove("all_classes");
            _cache.Remove("active_classes");
            
            return true;
        }

        public async Task<IEnumerable<LopHoc>> GetActiveClassesAsync()
        {
            const string cacheKey = "active_classes";
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<LopHoc>? classes))
            {
                classes = await _lopHocRepository.GetActiveClassesAsync();
                _cache.Set(cacheKey, classes, TimeSpan.FromMinutes(5));
            }
            return classes ?? new List<LopHoc>();
        }

        public async Task<IEnumerable<LopHoc>> GetClassesByTrainerAsync(int hlvId)
        {
            return await _lopHocRepository.GetClassesByTrainerAsync(hlvId);
        }

        public async Task<bool> IsClassAvailableAsync(int lopHocId, DateTime date)
        {
            var lopHoc = await _lopHocRepository.GetByIdAsync(lopHocId);
            if (lopHoc == null || lopHoc.TrangThai != "OPEN") return false;

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

        public async Task GenerateScheduleAsync(int lopHocId, DateTime startDate, DateTime endDate)
        {
            var lopHoc = await _lopHocRepository.GetByIdAsync(lopHocId);
            if (lopHoc == null) return;

            var thuTrongTuan = lopHoc.ThuTrongTuan.Split(',').Select(t => t.Trim()).ToList();
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                var dayOfWeek = GetVietnameseDayOfWeek(currentDate.DayOfWeek);
                if (thuTrongTuan.Contains(dayOfWeek))
                {
                    // Check if schedule already exists
                    var existingSchedule = await _unitOfWork.Context.LichLops
                        .FirstOrDefaultAsync(l => l.LopHocId == lopHocId && l.Ngay == currentDate);

                    if (existingSchedule == null)
                    {
                        var lichLop = new LichLop
                        {
                            LopHocId = lopHocId,
                            Ngay = currentDate,
                            GioBatDau = lopHoc.GioBatDau,
                            GioKetThuc = lopHoc.GioKetThuc,
                            TrangThai = "SCHEDULED"
                        };

                        await _unitOfWork.Context.LichLops.AddAsync(lichLop);
                    }
                }
                currentDate = currentDate.AddDays(1);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<LichLop>> GetClassScheduleAsync(int lopHocId, DateTime startDate, DateTime endDate)
        {
            return await _unitOfWork.Context.LichLops
                .Include(l => l.LopHoc)
                .Where(l => l.LopHocId == lopHocId && l.Ngay >= startDate && l.Ngay <= endDate)
                .OrderBy(l => l.Ngay)
                .ThenBy(l => l.GioBatDau)
                .ToListAsync();
        }

        public async Task<bool> CancelClassAsync(int lichLopId, string reason)
        {
            var lichLop = await _unitOfWork.Context.LichLops.FindAsync(lichLopId);
            if (lichLop == null) return false;

            lichLop.TrangThai = "CANCELED";
            await _unitOfWork.SaveChangesAsync();

            // TODO: Send notifications to booked members
            return true;
        }

        private string GetVietnameseDayOfWeek(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Thứ 2",
                DayOfWeek.Tuesday => "Thứ 3",
                DayOfWeek.Wednesday => "Thứ 4",
                DayOfWeek.Thursday => "Thứ 5",
                DayOfWeek.Friday => "Thứ 6",
                DayOfWeek.Saturday => "Thứ 7",
                DayOfWeek.Sunday => "Chủ nhật",
                _ => ""
            };
        }
    }
}
