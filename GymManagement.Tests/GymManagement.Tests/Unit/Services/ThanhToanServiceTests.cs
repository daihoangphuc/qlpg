using Moq;
using Xunit;
using GymManagement.Web.Services;
using GymManagement.Web.Data.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using GymManagement.Web.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System;
using Microsoft.EntityFrameworkCore;
using GymManagement.Web.Data;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GymManagement.Tests.Unit.Services
{
    public class ThanhToanServiceTests
    {
        private readonly Mock<IThanhToanRepository> _thanhToanRepositoryMock;
        private readonly Mock<IDangKyRepository> _dangKyRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IThongBaoService> _thongBaoServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<ThanhToanService>> _loggerMock;
        private readonly ThanhToanService _thanhToanService;

        public ThanhToanServiceTests()
        {
            _thanhToanRepositoryMock = new Mock<IThanhToanRepository>();
            _dangKyRepositoryMock = new Mock<IDangKyRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _thongBaoServiceMock = new Mock<IThongBaoService>();
            _emailServiceMock = new Mock<IEmailService>();
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<ThanhToanService>>();

            _thanhToanService = new ThanhToanService(
                _thanhToanRepositoryMock.Object,
                _dangKyRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _thongBaoServiceMock.Object,
                _emailServiceMock.Object,
                _configurationMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllPayments()
        {
            // Arrange
            var expectedPayments = new List<ThanhToan> { new ThanhToan(), new ThanhToan() };
            _thanhToanRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(expectedPayments);

            // Act
            var result = await _thanhToanService.GetAllAsync();

            // Assert
            Assert.Equal(expectedPayments.Count, result.Count());
            _thanhToanRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsPayment()
        {
            // Arrange
            var expectedPayment = new ThanhToan { ThanhToanId = 1 };
            _thanhToanRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(expectedPayment);

            // Act
            var result = await _thanhToanService.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPayment.ThanhToanId, result.ThanhToanId);
            _thanhToanRepositoryMock.Verify(repo => repo.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            _thanhToanRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ThanhToan)null);

            // Act
            var result = await _thanhToanService.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_ValidPayment_ReturnsCreatedPayment()
        {
            // Arrange
            var newPayment = new ThanhToan { SoTien = 100000 };
            var createdPayment = new ThanhToan { ThanhToanId = 1, SoTien = 100000 };
            _thanhToanRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<ThanhToan>())).ReturnsAsync(createdPayment);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _thanhToanService.CreateAsync(newPayment);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdPayment.ThanhToanId, result.ThanhToanId);
            _thanhToanRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<ThanhToan>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ValidPayment_ReturnsUpdatedPayment()
        {
            // Arrange
            var payment = new ThanhToan { ThanhToanId = 1, SoTien = 200000 };
            _thanhToanRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<ThanhToan>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _thanhToanService.UpdateAsync(payment);

            // Assert
            Assert.Equal(payment, result);
            _thanhToanRepositoryMock.Verify(repo => repo.UpdateAsync(payment), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ExistingId_ReturnsTrue()
        {
            // Arrange
            var payment = new ThanhToan { ThanhToanId = 1 };
            _thanhToanRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(payment);
            _thanhToanRepositoryMock.Setup(repo => repo.DeleteAsync(It.IsAny<ThanhToan>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _thanhToanService.DeleteAsync(1);

            // Assert
            Assert.True(result);
            _thanhToanRepositoryMock.Verify(repo => repo.GetByIdAsync(1), Times.Once);
            _thanhToanRepositoryMock.Verify(repo => repo.DeleteAsync(payment), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingId_ReturnsFalse()
        {
            // Arrange
            _thanhToanRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ThanhToan)null);

            // Act
            var result = await _thanhToanService.DeleteAsync(999);

            // Assert
            Assert.False(result);
            _thanhToanRepositoryMock.Verify(repo => repo.GetByIdAsync(999), Times.Once);
            _thanhToanRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<ThanhToan>()), Times.Never);
        }
        [Fact]
        public async Task GetByRegistrationIdAsync_ValidRegistrationId_ReturnsPayments()
        {
            // Arrange
            var registrationId = 1;
            var payments = new List<ThanhToan> { new ThanhToan { ThanhToanId = 1 } };
            _thanhToanRepositoryMock.Setup(repo => repo.GetByDangKyIdAsync(registrationId)).ReturnsAsync(payments);

            // Act
            var result = await _thanhToanService.GetByRegistrationIdAsync(registrationId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            _thanhToanRepositoryMock.Verify(repo => repo.GetByDangKyIdAsync(registrationId), Times.Once);
        }

        [Fact]
        public async Task GetByMemberIdAsync_ValidMemberId_ReturnsPayments()
        {
            // Arrange
            var memberId = 1;
            var registrations = new List<DangKy> { new DangKy { DangKyId = 1 } };
            var payments = new List<ThanhToan> { new ThanhToan { ThanhToanId = 1 } };
            _dangKyRepositoryMock.Setup(repo => repo.GetByMemberIdAsync(memberId)).ReturnsAsync(registrations);
            _thanhToanRepositoryMock.Setup(repo => repo.GetByDangKyIdAsync(1)).ReturnsAsync(payments);

            // Act
            var result = await _thanhToanService.GetByMemberIdAsync(memberId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            _dangKyRepositoryMock.Verify(repo => repo.GetByMemberIdAsync(memberId), Times.Once);
            _thanhToanRepositoryMock.Verify(repo => repo.GetByDangKyIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetPendingPaymentsAsync_ShouldReturnPendingPayments()
        {
            // Arrange
            var payments = new List<ThanhToan> { new ThanhToan { ThanhToanId = 1, TrangThai = "PENDING" } };
            _thanhToanRepositoryMock.Setup(repo => repo.GetPendingPaymentsAsync()).ReturnsAsync(payments);

            // Act
            var result = await _thanhToanService.GetPendingPaymentsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            _thanhToanRepositoryMock.Verify(repo => repo.GetPendingPaymentsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetSuccessfulPaymentsAsync_ShouldReturnSuccessfulPayments()
        {
            // Arrange
            var payments = new List<ThanhToan> { new ThanhToan { ThanhToanId = 1, TrangThai = "SUCCESS" } };
            _thanhToanRepositoryMock.Setup(repo => repo.GetSuccessfulPaymentsAsync()).ReturnsAsync(payments);

            // Act
            var result = await _thanhToanService.GetSuccessfulPaymentsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            _thanhToanRepositoryMock.Verify(repo => repo.GetSuccessfulPaymentsAsync(), Times.Once);
        }

        [Fact]
        public async Task CreatePaymentAsync_ValidData_ReturnsCreatedPayment()
        {
            // Arrange
            var thanhToan = new ThanhToan { ThanhToanId = 1, DangKyId = 1, SoTien = 100000, PhuongThuc = "Cash" };
            _thanhToanRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<ThanhToan>())).ReturnsAsync(thanhToan);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _thanhToanService.CreatePaymentAsync(1, 100000, "Cash");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(thanhToan.ThanhToanId, result.ThanhToanId);
            _thanhToanRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<ThanhToan>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ProcessCashPaymentAsync_ValidPaymentId_ReturnsTrue()
        {
            // Arrange
            var thanhToan = new ThanhToan { ThanhToanId = 1, TrangThai = "PENDING", DangKyId = 1 };
            var dangKy = new DangKy { DangKyId = 1, TrangThai = "PENDING_PAYMENT" };

            // Tạo in-memory database cho test này
            var options = new DbContextOptionsBuilder<GymDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_ProcessCashPayment")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            using var context = new GymDbContext(options);
            context.DangKys.Add(dangKy);
            await context.SaveChangesAsync();

            _thanhToanRepositoryMock.Setup(repo => repo.GetPaymentWithGatewayAsync(1)).ReturnsAsync(thanhToan);
            _unitOfWorkMock.Setup(uow => uow.Context).Returns(context);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);
            
            // Mock dependencies for SendPaymentSuccessNotifications private method
            _dangKyRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(dangKy);
            _thongBaoServiceMock.Setup(s => s.CreateNotificationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ThongBao { ThongBaoId = 1 });

            // Act
            var result = await _thanhToanService.ProcessCashPaymentAsync(1);

            // Assert
            Assert.True(result);
            Assert.Equal("SUCCESS", thanhToan.TrangThai);
            Assert.Equal("ACTIVE", dangKy.TrangThai);
            _thanhToanRepositoryMock.Verify(repo => repo.GetPaymentWithGatewayAsync(1), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }
    }
}
