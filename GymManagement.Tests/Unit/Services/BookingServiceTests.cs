using FluentAssertions;
using GymManagement.Web.Data;
using GymManagement.Web.Data.Models;
using GymManagement.Web.Data.Repositories;
using GymManagement.Web.Services;
using Microsoft.EntityFrameworkCore;
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
    }
}

