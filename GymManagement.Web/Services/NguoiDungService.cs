using GymManagement.Web.Data;
using GymManagement.Web.Data.Models;
using GymManagement.Web.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Web.Services
{
    public class NguoiDungService : INguoiDungService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NguoiDungService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<NguoiDungDto?> GetByIdAsync(int id)
        {
            var nguoiDung = await _unitOfWork.NguoiDungs.GetByIdAsync(id);
            return nguoiDung != null ? MapToDto(nguoiDung) : null;
        }

        public async Task<IEnumerable<NguoiDungDto>> GetAllAsync()
        {
            var nguoiDungs = await _unitOfWork.NguoiDungs.GetAllAsync();
            return nguoiDungs.Select(MapToDto);
        }

        public async Task<(IEnumerable<NguoiDungDto> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null, string? loaiNguoiDung = null)
        {
            var filter = BuildFilter(searchTerm, loaiNguoiDung);
            var orderBy = BuildOrderBy();

            var (items, totalCount) = await _unitOfWork.NguoiDungs.GetPagedAsync(
                pageNumber, pageSize, filter, orderBy);

            return (items.Select(MapToDto), totalCount);
        }

        public async Task<NguoiDungDto> CreateAsync(CreateNguoiDungDto createDto)
        {
            // Validation
            await ValidateCreateAsync(createDto);

            var nguoiDung = new NguoiDung
            {
                LoaiNguoiDung = createDto.LoaiNguoiDung,
                Ho = createDto.Ho,
                Ten = createDto.Ten,
                GioiTinh = createDto.GioiTinh,
                NgaySinh = createDto.NgaySinh,
                SoDienThoai = createDto.SoDienThoai,
                Email = createDto.Email,
                NgayThamGia = DateOnly.FromDateTime(DateTime.Now),
                TrangThai = "ACTIVE"
            };

            await _unitOfWork.NguoiDungs.AddAsync(nguoiDung);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(nguoiDung);
        }

        public async Task<NguoiDungDto> UpdateAsync(UpdateNguoiDungDto updateDto)
        {
            var nguoiDung = await _unitOfWork.NguoiDungs.GetByIdAsync(updateDto.NguoiDungId);
            if (nguoiDung == null)
                throw new ArgumentException("Người dùng không tồn tại");

            // Validation
            await ValidateUpdateAsync(updateDto);

            // Update properties
            nguoiDung.LoaiNguoiDung = updateDto.LoaiNguoiDung;
            nguoiDung.Ho = updateDto.Ho;
            nguoiDung.Ten = updateDto.Ten;
            nguoiDung.GioiTinh = updateDto.GioiTinh;
            nguoiDung.NgaySinh = updateDto.NgaySinh;
            nguoiDung.SoDienThoai = updateDto.SoDienThoai;
            nguoiDung.Email = updateDto.Email;
            nguoiDung.TrangThai = updateDto.TrangThai;

            await _unitOfWork.NguoiDungs.UpdateAsync(nguoiDung);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(nguoiDung);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var nguoiDung = await _unitOfWork.NguoiDungs.GetByIdAsync(id);
            if (nguoiDung == null)
                return false;

            await _unitOfWork.NguoiDungs.DeleteAsync(nguoiDung);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<NguoiDungDto>> GetByLoaiNguoiDungAsync(string loaiNguoiDung)
        {
            var nguoiDungs = await _unitOfWork.NguoiDungs.GetByLoaiNguoiDungAsync(loaiNguoiDung);
            return nguoiDungs.Select(MapToDto);
        }

        public async Task<NguoiDungDto?> GetByEmailAsync(string email)
        {
            var nguoiDung = await _unitOfWork.NguoiDungs.GetByEmailAsync(email);
            return nguoiDung != null ? MapToDto(nguoiDung) : null;
        }

        public async Task<NguoiDungDto?> GetBySoDienThoaiAsync(string soDienThoai)
        {
            var nguoiDung = await _unitOfWork.NguoiDungs.GetBySoDienThoaiAsync(soDienThoai);
            return nguoiDung != null ? MapToDto(nguoiDung) : null;
        }

        public async Task<IEnumerable<NguoiDungDto>> GetActiveUsersAsync()
        {
            var nguoiDungs = await _unitOfWork.NguoiDungs.GetActiveUsersAsync();
            return nguoiDungs.Select(MapToDto);
        }

        public async Task<IEnumerable<NguoiDungDto>> GetHuanLuyenViensAsync()
        {
            var nguoiDungs = await _unitOfWork.NguoiDungs.GetHuanLuyenViensAsync();
            return nguoiDungs.Select(MapToDto);
        }

        public async Task<IEnumerable<NguoiDungDto>> GetThanhViensAsync()
        {
            var nguoiDungs = await _unitOfWork.NguoiDungs.GetThanhViensAsync();
            return nguoiDungs.Select(MapToDto);
        }

        public async Task<bool> IsEmailExistsAsync(string email, int? excludeId = null)
        {
            var nguoiDung = await _unitOfWork.NguoiDungs.GetByEmailAsync(email);
            return nguoiDung != null && (excludeId == null || nguoiDung.NguoiDungId != excludeId);
        }

        public async Task<bool> IsSoDienThoaiExistsAsync(string soDienThoai, int? excludeId = null)
        {
            var nguoiDung = await _unitOfWork.NguoiDungs.GetBySoDienThoaiAsync(soDienThoai);
            return nguoiDung != null && (excludeId == null || nguoiDung.NguoiDungId != excludeId);
        }

        public async Task<bool> DeactivateUserAsync(int id)
        {
            var nguoiDung = await _unitOfWork.NguoiDungs.GetByIdAsync(id);
            if (nguoiDung == null)
                return false;

            nguoiDung.TrangThai = "INACTIVE";
            await _unitOfWork.NguoiDungs.UpdateAsync(nguoiDung);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateUserAsync(int id)
        {
            var nguoiDung = await _unitOfWork.NguoiDungs.GetByIdAsync(id);
            if (nguoiDung == null)
                return false;

            nguoiDung.TrangThai = "ACTIVE";
            await _unitOfWork.NguoiDungs.UpdateAsync(nguoiDung);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // Private helper methods
        private static NguoiDungDto MapToDto(NguoiDung nguoiDung)
        {
            return new NguoiDungDto
            {
                NguoiDungId = nguoiDung.NguoiDungId,
                LoaiNguoiDung = nguoiDung.LoaiNguoiDung,
                Ho = nguoiDung.Ho,
                Ten = nguoiDung.Ten,
                GioiTinh = nguoiDung.GioiTinh,
                NgaySinh = nguoiDung.NgaySinh,
                SoDienThoai = nguoiDung.SoDienThoai,
                Email = nguoiDung.Email,
                NgayThamGia = nguoiDung.NgayThamGia,
                TrangThai = nguoiDung.TrangThai
            };
        }

        private static System.Linq.Expressions.Expression<Func<NguoiDung, bool>>? BuildFilter(
            string? searchTerm, string? loaiNguoiDung)
        {
            if (string.IsNullOrEmpty(searchTerm) && string.IsNullOrEmpty(loaiNguoiDung))
                return null;

            return x => (string.IsNullOrEmpty(searchTerm) || 
                        x.Ho.Contains(searchTerm) || 
                        (x.Ten != null && x.Ten.Contains(searchTerm)) ||
                        (x.Email != null && x.Email.Contains(searchTerm)) ||
                        (x.SoDienThoai != null && x.SoDienThoai.Contains(searchTerm))) &&
                       (string.IsNullOrEmpty(loaiNguoiDung) || x.LoaiNguoiDung == loaiNguoiDung);
        }

        private static Func<IQueryable<NguoiDung>, IOrderedQueryable<NguoiDung>> BuildOrderBy()
        {
            return query => query.OrderBy(x => x.Ho).ThenBy(x => x.Ten);
        }

        private async Task ValidateCreateAsync(CreateNguoiDungDto createDto)
        {
            if (!string.IsNullOrEmpty(createDto.Email))
            {
                if (await IsEmailExistsAsync(createDto.Email))
                    throw new ArgumentException("Email đã tồn tại");
            }

            if (!string.IsNullOrEmpty(createDto.SoDienThoai))
            {
                if (await IsSoDienThoaiExistsAsync(createDto.SoDienThoai))
                    throw new ArgumentException("Số điện thoại đã tồn tại");
            }
        }

        private async Task ValidateUpdateAsync(UpdateNguoiDungDto updateDto)
        {
            if (!string.IsNullOrEmpty(updateDto.Email))
            {
                if (await IsEmailExistsAsync(updateDto.Email, updateDto.NguoiDungId))
                    throw new ArgumentException("Email đã tồn tại");
            }

            if (!string.IsNullOrEmpty(updateDto.SoDienThoai))
            {
                if (await IsSoDienThoaiExistsAsync(updateDto.SoDienThoai, updateDto.NguoiDungId))
                    throw new ArgumentException("Số điện thoại đã tồn tại");
            }
        }
    }
}
