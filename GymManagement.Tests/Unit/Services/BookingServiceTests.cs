using FluentAssertions;
using GymManagement.Web.Data;
using GymManagement.Web.Data.Models;
using GymManagement.Web.Data.Repositories;
using GymManagement.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GymManagement.Tests.Unit.Services
{
    public class BookingServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IBookingRepository> _bookingRepositoryMock;
        private readonly Mock<ILopHocRepository> _lopHocRepositoryMock;
        private readonly Mock<IThongBaoService> _thongBaoServiceMock;
        private readonly BookingService _bookingService;

        public BookingServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _bookingRepositoryMock = new Mock<IBookingRepository>();
            _lopHocRepositoryMock = new Mock<ILopHocRepository>();
            _thongBaoServiceMock = new Mock<IThongBaoService>();
            _bookingService = new BookingService(_unitOfWorkMock.Object, _bookingRepositoryMock.Object, _lopHocRepositoryMock.Object, _thongBaoServiceMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllBookings()
        {
            // Arrange
            var bookings = new List<Booking> { new Booking { BookingId = 1 }, new Booking { BookingId = 2 } };
            _bookingRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(bookings);

            // Act
            var result = await _bookingService.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsBooking()
        {
            // Arrange
            var booking = new Booking { BookingId = 1 };
            _bookingRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(booking);

            // Act
            var result = await _bookingService.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result?.BookingId.Should().Be(1);
        }

        [Fact]
        public async Task BookClassAsync_ValidInputs_ReturnsTrue()
        {
            // Arrange
            var lopHoc = new LopHoc { LopHocId = 1, SucChua = 30, TrangThai = "OPEN", TenLop = "Yoga" };
            _lopHocRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(lopHoc);
            _bookingRepositoryMock.Setup(repo => repo.CountBookingsForClassAsync(1, It.IsAny<DateTime>())).ReturnsAsync(0);
            _bookingRepositoryMock.Setup(repo => repo.HasBookingAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<DateTime>())).ReturnsAsync(false);
            _bookingRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Booking>())).ReturnsAsync((Booking b) => b);
            
            // Setup UnitOfWork context
            var options = new DbContextOptionsBuilder<GymDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            var context = new GymDbContext(options);
            _unitOfWorkMock.Setup(uow => uow.Context).Returns(context);

            // Act
            var result = await _bookingService.BookClassAsync(1, 1, DateTime.Today);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CreateAsync_ValidBooking_ReturnsBooking()
        {
            // Arrange
            var booking = new Booking { BookingId = 1 };
            _bookingRepositoryMock.Setup(repo => repo.AddAsync(booking)).ReturnsAsync(booking);

            // Act
            var result = await _bookingService.CreateAsync(booking);

            // Assert
            result.Should().NotBeNull();
            result.BookingId.Should().Be(1);
        }

        #region BookClassWithTransactionAsync Tests - Race Condition Prevention

        private async Task<GymDbContext> CreateSqliteContextAsync()
        {
            var options = new DbContextOptionsBuilder<GymDbContext>()
                .UseSqlite($"Data Source=:memory:")
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var context = new GymDbContext(options);
            await context.Database.OpenConnectionAsync();
            await context.Database.EnsureCreatedAsync();
            return context;
        }

        [Fact]
        public async Task BookClassWithTransactionAsync_ValidInputs_ReturnsSuccess()
        {
            // Arrange
            using var context = await CreateSqliteContextAsync();

            // Seed test data
            var lopHoc = new LopHoc
            {
                LopHocId = 1,
                TenLop = "Yoga Morning",
                SucChua = 20,
                TrangThai = "OPEN",
                GioBatDau = new TimeOnly(7, 0),
                GioKetThuc = new TimeOnly(8, 0),
                ThuTrongTuan = "Monday,Wednesday,Friday"
            };

            var nguoiDung = new NguoiDung
            {
                NguoiDungId = 1,
                Ho = "Test",
                Ten = "User",
                Email = "test@example.com",
                LoaiNguoiDung = "THANHVIEN",
                NgayThamGia = DateOnly.FromDateTime(DateTime.Today)
            };

            context.LopHocs.Add(lopHoc);
            context.NguoiDungs.Add(nguoiDung);
            await context.SaveChangesAsync();

            _unitOfWorkMock.Setup(uow => uow.Context).Returns(context);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).Returns(Task.FromResult(0));

            // Act
            var result = await _bookingService.BookClassWithTransactionAsync(1, 1, DateTime.Today.AddDays(1), "Test booking");

            // Assert
            result.Success.Should().BeTrue();
            result.ErrorMessage.Should().Be("Đặt lịch thành công");

            // Verify booking was created
            var booking = await context.Bookings.FirstOrDefaultAsync();
            booking.Should().NotBeNull();
            booking!.ThanhVienId.Should().Be(1);
            booking.LopHocId.Should().Be(1);
            booking.TrangThai.Should().Be("BOOKED");
        }

        [Fact]
        public async Task BookClassWithTransactionAsync_PastDate_ReturnsFailure()
        {
            // Arrange
            using var context = await CreateSqliteContextAsync();
            _unitOfWorkMock.Setup(uow => uow.Context).Returns(context);

            // Act
            var result = await _bookingService.BookClassWithTransactionAsync(1, 1, DateTime.Today.AddDays(-1), "Test booking");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Không thể đặt lịch cho ngày trong quá khứ");
        }

        [Fact]
        public async Task BookClassWithTransactionAsync_ClassNotFound_ReturnsFailure()
        {
            // Arrange
            using var context = await CreateSqliteContextAsync();
            _unitOfWorkMock.Setup(uow => uow.Context).Returns(context);

            // Act
            var result = await _bookingService.BookClassWithTransactionAsync(1, 999, DateTime.Today.AddDays(1), "Test booking");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Lớp học không tồn tại");
        }

        [Fact]
        public async Task BookClassWithTransactionAsync_ClassClosed_ReturnsFailure()
        {
            // Arrange
            using var context = await CreateSqliteContextAsync();

            var lopHoc = new LopHoc
            {
                LopHocId = 1,
                TenLop = "Closed Class",
                SucChua = 20,
                TrangThai = "CLOSED",
                GioBatDau = new TimeOnly(7, 0),
                GioKetThuc = new TimeOnly(8, 0),
                ThuTrongTuan = "Monday"
            };

            context.LopHocs.Add(lopHoc);
            await context.SaveChangesAsync();

            _unitOfWorkMock.Setup(uow => uow.Context).Returns(context);

            // Act
            var result = await _bookingService.BookClassWithTransactionAsync(1, 1, DateTime.Today.AddDays(1), "Test booking");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Lớp học đã đóng hoặc không khả dụng");
        }

        [Fact]
        public async Task BookClassWithTransactionAsync_DuplicateBooking_ReturnsFailure()
        {
            // Arrange
            using var context = await CreateSqliteContextAsync();

            var lopHoc = new LopHoc
            {
                LopHocId = 1,
                TenLop = "Yoga Morning",
                SucChua = 20,
                TrangThai = "OPEN",
                GioBatDau = new TimeOnly(7, 0),
                GioKetThuc = new TimeOnly(8, 0),
                ThuTrongTuan = "Monday"
            };

            var existingBooking = new Booking
            {
                BookingId = 1,
                ThanhVienId = 1,
                LopHocId = 1,
                Ngay = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                TrangThai = "BOOKED"
            };

            context.LopHocs.Add(lopHoc);
            context.Bookings.Add(existingBooking);
            await context.SaveChangesAsync();

            _unitOfWorkMock.Setup(uow => uow.Context).Returns(context);

            // Act
            var result = await _bookingService.BookClassWithTransactionAsync(1, 1, DateTime.Today.AddDays(1), "Test booking");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Bạn đã đặt lịch cho lớp này trong ngày này rồi");
        }

        [Fact]
        public async Task BookClassWithTransactionAsync_ClassFull_ReturnsFailure()
        {
            // Arrange
            using var context = await CreateSqliteContextAsync();

            var lopHoc = new LopHoc
            {
                LopHocId = 1,
                TenLop = "Small Class",
                SucChua = 1, // Only 1 slot
                TrangThai = "OPEN",
                GioBatDau = new TimeOnly(7, 0),
                GioKetThuc = new TimeOnly(8, 0),
                ThuTrongTuan = "Monday"
            };

            var existingBooking = new Booking
            {
                BookingId = 1,
                ThanhVienId = 2, // Different member
                LopHocId = 1,
                Ngay = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                TrangThai = "BOOKED"
            };

            context.LopHocs.Add(lopHoc);
            context.Bookings.Add(existingBooking);
            await context.SaveChangesAsync();

            _unitOfWorkMock.Setup(uow => uow.Context).Returns(context);

            // Act
            var result = await _bookingService.BookClassWithTransactionAsync(1, 1, DateTime.Today.AddDays(1), "Test booking");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Lớp học đã đầy, vui lòng chọn lớp khác");
        }

        #endregion

        #region Concurrent Booking Tests

        [Fact]
        public async Task BookClassWithTransactionAsync_ConcurrentBookings_OnlyOneSucceeds()
        {
            // Arrange
            using var context = await CreateSqliteContextAsync();

            var lopHoc = new LopHoc
            {
                LopHocId = 1,
                TenLop = "Limited Class",
                SucChua = 1, // Only 1 slot available
                TrangThai = "OPEN",
                GioBatDau = new TimeOnly(7, 0),
                GioKetThuc = new TimeOnly(8, 0),
                ThuTrongTuan = "Monday"
            };

            context.LopHocs.Add(lopHoc);
            await context.SaveChangesAsync();

            _unitOfWorkMock.Setup(uow => uow.Context).Returns(context);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).Returns(Task.FromResult(0));

            // Act - Simulate concurrent bookings
            var task1 = _bookingService.BookClassWithTransactionAsync(1, 1, DateTime.Today.AddDays(1), "User 1 booking");
            var task2 = _bookingService.BookClassWithTransactionAsync(2, 1, DateTime.Today.AddDays(1), "User 2 booking");

            var results = await Task.WhenAll(task1, task2);

            // Assert
            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count(r => !r.Success);

            successCount.Should().Be(1, "Only one booking should succeed");
            failureCount.Should().Be(1, "One booking should fail due to capacity");

            // Verify only one booking exists in database
            var bookingCount = await context.Bookings.CountAsync();
            bookingCount.Should().Be(1, "Only one booking should be persisted");
        }

        #endregion
    }
}

