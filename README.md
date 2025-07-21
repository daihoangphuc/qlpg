# 🏋️ Gym Management System

Hệ thống quản lý phòng gym hiện đại và chuyên nghiệp được xây dựng bằng ASP.NET Core 8.0, Entity Framework Core, và Tailwind CSS.

## ✨ Tính năng chính

### 👥 Quản lý người dùng
- Đăng ký/đăng nhập với ASP.NET Core Identity
- Phân quyền theo vai trò (Admin, Manager, Staff, Trainer, Member)
- Quản lý thông tin cá nhân và avatar
- Hệ thống thông báo tích hợp

### 📦 Quản lý gói tập
- Tạo và quản lý các gói tập khác nhau
- Thiết lập giá và thời hạn linh hoạt
- Hệ thống khuyến mãi và giảm giá

### 🏃‍♂️ Quản lý lớp học
- Tạo lịch học tự động
- Quản lý sức chứa lớp học
- Đăng ký và hủy lớp học
- Theo dõi tình trạng lớp học

### ✅ Điểm danh thông minh
- Điểm danh thủ công
- Nhận diện khuôn mặt (Face Recognition)
- Theo dõi lịch sử điểm danh
- Báo cáo thống kê điểm danh

### 💳 Thanh toán đa dạng
- Thanh toán tiền mặt
- Tích hợp VNPay
- Quản lý hóa đơn và biên lai
- Hệ thống hoàn tiền

### 📊 Báo cáo và thống kê
- Dashboard tổng quan
- Báo cáo doanh thu
- Thống kê thành viên
- Phân tích hiệu suất

### 💰 Quản lý lương
- Tính lương tự động cho HLV
- Hệ thống hoa hồng
- Báo cáo chi phí nhân sự

## 🛠️ Công nghệ sử dụng

- **Backend**: ASP.NET Core 8.0
- **Database**: SQL Server với Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Razor Pages + Tailwind CSS + Flowbite
- **Logging**: Serilog
- **Email**: MailKit
- **Payment**: VNPay Integration
- **Charts**: Chart.js

## 📋 Yêu cầu hệ thống

- .NET 8.0 SDK
- SQL Server (LocalDB hoặc SQL Server Express)
- Visual Studio 2022 hoặc VS Code
- Node.js (cho Tailwind CSS - tùy chọn)

## 🚀 Cài đặt và chạy

### 1. Clone repository
```bash
git clone https://github.com/daihoangphuc/qlpg.git
cd qlpg
```

### 2. Cấu hình database
Cập nhật connection string trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "GymDb": "Server=(localdb)\\mssqllocaldb;Database=GymManagementDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 3. Cài đặt packages
```bash
dotnet restore
```

### 4. Tạo database
```bash
dotnet ef database update
```

### 5. Chạy ứng dụng
```bash
dotnet run
```

Ứng dụng sẽ chạy tại: `https://localhost:7000`

## 👤 Tài khoản mặc định

Sau khi khởi tạo database, hệ thống sẽ tạo các tài khoản mặc định:

| Tài khoản | Mật khẩu | Vai trò |
|-----------|----------|---------|
| admin | Admin@123 | Admin |
| trainer1 | Trainer@123 | Trainer |
| trainer2 | Trainer@123 | Trainer |
| member1 | Member@123 | Member |

## 📁 Cấu trúc dự án

```
GymManagement.Web/
├── Controllers/          # MVC Controllers
├── Data/
│   ├── Models/          # Entity Models
│   ├── Repositories/    # Repository Pattern
│   └── GymDbContext.cs  # Database Context
├── Services/            # Business Logic Services
├── Views/               # Razor Views
├── wwwroot/            # Static Files
├── Models/
│   ├── DTOs/           # Data Transfer Objects
│   └── ViewModels/     # View Models
└── Program.cs          # Application Entry Point
```

## 🔧 Cấu hình

### Email Settings
Cập nhật cấu hình email trong `appsettings.json`:
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "Gym Management System"
  }
}
```

### VNPay Settings
Cấu hình VNPay cho thanh toán online:
```json
{
  "VnPay": {
    "TmnCode": "YOUR_TMN_CODE",
    "HashSecret": "YOUR_HASH_SECRET",
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"
  }
}
```

## 📱 Tính năng nổi bật

### 🎨 Giao diện hiện đại
- Responsive design với Tailwind CSS
- Dark/Light mode
- Mobile-friendly
- Accessibility support

### 🔐 Bảo mật
- ASP.NET Core Identity
- Role-based authorization
- HTTPS enforcement
- Input validation

### 📈 Hiệu suất
- Entity Framework Core optimization
- Memory caching
- Lazy loading
- Connection pooling

### 🔍 Tìm kiếm và lọc
- Real-time search
- Advanced filtering
- Pagination
- Sorting

## 🤝 Đóng góp

1. Fork repository
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

## 📄 License

Dự án này được phân phối dưới giấy phép MIT. Xem file `LICENSE` để biết thêm chi tiết.

## 📞 Liên hệ

- **Email**: 90683814+daihoangphuc@users.noreply.github.com
- **GitHub**: [@daihoangphuc](https://github.com/daihoangphuc)

## 🙏 Acknowledgments

- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Tailwind CSS](https://tailwindcss.com/)
- [Flowbite](https://flowbite.com/)
- [Chart.js](https://www.chartjs.org/)

---

⭐ Nếu dự án này hữu ích, hãy cho một star nhé!
