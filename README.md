# 🏋️‍♂️ Gym Management System

<div align="center">

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4?style=for-the-badge&logo=dotnet)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-512BD4?style=for-the-badge&logo=microsoft)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server)
![Tailwind CSS](https://img.shields.io/badge/Tailwind%20CSS-06B6D4?style=for-the-badge&logo=tailwindcss&logoColor=white)
![VNPay](https://img.shields.io/badge/VNPay-Integration-00A86B?style=for-the-badge)

**Hệ thống quản lý phòng gym hiện đại và toàn diện**

[🚀 Demo](#-demo) • [📚 Tài liệu](#-tài-liệu) • [🛠️ Cài đặt](#️-cài-đặt) • [🤝 Đóng góp](#-đóng-góp)

</div>

---

## 🌟 Tổng quan

**Gym Management System** là một ứng dụng web toàn diện được thiết kế để quản lý mọi hoạt động của phòng gym hiện đại. Từ quản lý thành viên, lớp học, thanh toán đến báo cáo tài chính - tất cả được tích hợp trong một hệ thống duy nhất với giao diện thân thiện và hiệu suất cao.

### 🎯 Điểm nổi bật

- **🤖 AI-Powered**: Tích hợp nhận diện khuôn mặt cho điểm danh tự động
- **💳 Thanh toán đa dạng**: Hỗ trợ VNPay và thanh toán trực tiếp  
- **📊 Phân tích thông minh**: Dashboard với biểu đồ thời gian thực
- **🔐 Bảo mật cao**: ASP.NET Core Identity với phân quyền chi tiết
- **📱 Responsive**: Giao diện tối ưu cho mọi thiết bị
- **⚡ Hiệu suất cao**: Caching và tối ưu hóa database

---

## ✨ Tính năng chính

### 👥 **Quản lý người dùng & Phân quyền**
- **5 cấp độ phân quyền**: Admin, Manager, Staff, Trainer, Member
- **Xác thực bảo mật**: ASP.NET Core Identity với mã hóa BCrypt
- **Quản lý hồ sơ**: Avatar, thông tin cá nhân, lịch sử hoạt động
- **Hệ thống thông báo**: Real-time notifications cho các sự kiện quan trọng

### 📦 **Quản lý gói tập & Dịch vụ**
```
🎯 Gói tập linh hoạt     📅 Thời hạn tùy chỉnh     💰 Định giá động
🎁 Khuyến mãi thông minh  🏷️ Mã giảm giá          📈 Phân tích hiệu quả
```

### 🏃‍♂️ **Quản lý lớp học thông minh**
- **Tự động tạo lịch**: Lập lịch học tự động theo template
- **Quản lý sức chứa**: Theo dõi số lượng học viên real-time
- **Đăng ký linh hoạt**: Đăng ký/hủy lớp học với xác nhận tức thì
- **Booking system**: Đặt chỗ trước với tính năng waiting list

### ✅ **Điểm danh AI-Powered**
- **Điểm danh truyền thống**: Check-in thủ công qua QR code
- **🤖 Face Recognition**: Nhận diện khuôn mặt với thuật toán ArcFace
- **📊 Analytics**: Thống kê tần suất tập luyện và xu hướng
- **📱 Mobile-friendly**: Điểm danh qua ứng dụng di động

### 💳 **Hệ thống thanh toán toàn diện**

| Phương thức | Tính năng | Trạng thái |
|-------------|-----------|------------|
| 💵 Tiền mặt | Thanh toán trực tiếp | ✅ Hoạt động |
| 🏦 VNPay | Thanh toán online | ✅ Tích hợp sẵn |
| 🧾 Hóa đơn | Tự động tạo receipt | ✅ Hoạt động |
| 💸 Hoàn tiền | Xử lý hoàn tiền | ✅ Hoạt động |

### 📊 **Business Intelligence & Báo cáo**
- **📈 Dashboard tổng quan**: Real-time metrics với Chart.js
- **💰 Báo cáo doanh thu**: Phân tích theo ngày/tháng/năm
- **👥 Thống kê thành viên**: Growth rate, retention analysis
- **🏆 Hiệu suất HLV**: Đánh giá KPI và hoa hồng

### 💼 **Quản lý nhân sự & Lương**
- **💰 Tính lương tự động**: Công thức linh hoạt cho từng vị trí
- **🎯 Hệ thống hoa hồng**: Commission tracking cho HLV
- **📊 Báo cáo HR**: Chi phí nhân sự và productivity metrics
- **⏰ Chấm công**: Tích hợp với hệ thống điểm danh

---

## 🛠️ Kiến trúc & Công nghệ

### **Backend Architecture**
```
🏗️ ASP.NET Core 8.0
├── 🗄️ Entity Framework Core (Code-First)
├── 🔐 ASP.NET Core Identity
├── 📦 Repository + Unit of Work Pattern
├── 🎯 Dependency Injection
├── 📝 Serilog Logging
└── 💾 Memory Caching
```

### **Frontend Stack**
```
🎨 Modern UI/UX
├── 🌊 Tailwind CSS 3.4
├── 🧩 Flowbite Components
├── 📊 Chart.js
├── ⚡ Vanilla JavaScript
└── 📱 Responsive Design
```

### **Database Design**
```
🗃️ SQL Server
├── 👥 19 Core Tables
├── 🔗 Foreign Key Constraints  
├── 📊 Indexed for Performance
├── 🔄 Migration Support
└── 🛡️ Data Integrity
```

### **External Integrations**
- **💳 VNPay Gateway**: Thanh toán online an toàn
- **📧 MailKit**: Hệ thống email SMTP
- **🤖 Face Recognition**: ArcFace algorithm ready
- **🏥 Health Checks**: Application monitoring

---

## 📊 Database Schema

<details>
<summary>👀 Xem sơ đồ cơ sở dữ liệu</summary>

### Core Entities (19 Tables)

**🔐 Security & Users**
- `VaiTro` - Roles and permissions
- `NguoiDung` - User profiles (Member/Trainer/Staff)  
- `TaiKhoan` - Authentication accounts

**📦 Services & Packages**
- `GoiTap` - Membership packages
- `KhuyenMai` - Promotions and discounts
- `LopHoc` - Fitness classes
- `LichLop` - Class schedules

**💰 Financial**
- `DangKy` - Registrations
- `ThanhToan` - Payments
- `ThanhToanGateway` - Payment gateways
- `BangLuong` - Payroll
- `CauHinhHoaHong` - Commission settings

**📋 Operations**
- `Booking` - Class bookings
- `DiemDanh` - Attendance records
- `BuoiTap` - Training sessions
- `BuoiHlv` - Trainer sessions

**🤖 AI & System**
- `MauMat` - Face recognition templates
- `ThongBao` - Notifications
- `LichSuAnh` - Image history

</details>

---

## 🚀 Cài đặt & Triển khai

### 📋 Yêu cầu hệ thống

| Thành phần | Phiên bản | Ghi chú |
|------------|-----------|---------|
| .NET SDK | 8.0+ | Required |
| SQL Server | 2019+ / LocalDB | Database |
| Node.js | 18+ | Cho Tailwind CSS |
| Visual Studio | 2022+ | Khuyến nghị |

### ⚡ Cài đặt nhanh

```bash
# 1. Clone repository
git clone https://github.com/daihoangphuc/qlpg.git
cd qlpg

# 2. Restore packages
dotnet restore
npm install

# 3. Cấu hình database
# Cập nhật connection string trong appsettings.json

# 4. Tạo database và seed data
dotnet ef database update

# 5. Build Tailwind CSS
npm run build-css

# 6. Chạy ứng dụng
dotnet run
```

🌐 **Truy cập**: `https://localhost:7000`

### 🔧 Cấu hình chi tiết

<details>
<summary>⚙️ Cấu hình Database</summary>

```json
{
  "ConnectionStrings": {
    "GymDb": "Server=localhost;Database=QLPG;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

</details>

<details>
<summary>📧 Cấu hình Email</summary>

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

</details>

<details>
<summary>💳 Cấu hình VNPay</summary>

```json
{
  "VnPay": {
    "TmnCode": "YOUR_TMN_CODE",
    "HashSecret": "YOUR_HASH_SECRET",
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "Command": "pay",
    "CurrCode": "VND",
    "Version": "2.1.0",
    "Locale": "vn"
  }
}
```

</details>

---

## 👤 Tài khoản Demo

| Vai trò | Username | Password | Quyền hạn |
|---------|----------|----------|-----------|
| 🔱 **Admin** | `admin` | `Admin@123` | Toàn quyền hệ thống |
| 👨‍💼 **Manager** | `manager1` | `Manager@123` | Quản lý vận hành |
| 👨‍🏫 **Trainer** | `trainer1` | `Trainer@123` | Quản lý lớp học |
| 👤 **Member** | `member1` | `Member@123` | Sử dụng dịch vụ |

---

## 📁 Cấu trúc dự án

```
GymManagement.Web/
├── 🎛️ Controllers/             # MVC Controllers (11 files)
│   ├── AuthController.cs       # Authentication
│   ├── HomeController.cs       # Dashboard & Landing
│   ├── NguoiDungController.cs  # User Management
│   ├── GoiTapController.cs     # Package Management
│   ├── LopHocController.cs     # Class Management
│   ├── BookingController.cs    # Booking System
│   ├── DiemDanhController.cs   # Attendance & Face Recognition
│   ├── ThanhToanController.cs  # Payment Processing
│   ├── BangLuongController.cs  # Payroll Management
│   ├── BaoCaoController.cs     # Reports & Analytics
│   └── DangKyController.cs     # Registration
├── 🗄️ Data/                   # Data Layer
│   ├── Models/                 # Entity Models (19 files)
│   ├── Repositories/           # Repository Pattern
│   ├── GymDbContext.cs         # EF Core Context
│   ├── UnitOfWork.cs           # Unit of Work
│   └── DbInitializer.cs        # Seed Data
├── 🎯 Services/                # Business Logic (22 files)
│   ├── INguoiDungService.cs    # User Service Interface
│   ├── NguoiDungService.cs     # User Business Logic
│   ├── IEmailService.cs        # Email Interface
│   ├── EmailService.cs         # Email Implementation
│   └── ...                     # Other services
├── 🎨 Views/                   # Razor Views
│   ├── Shared/                 # Layout & Partials
│   ├── Home/                   # Dashboard Views
│   ├── Auth/                   # Authentication Views
│   └── ...                     # Feature Views
├── 🌐 wwwroot/                 # Static Assets
│   ├── css/                    # Compiled CSS
│   ├── js/                     # JavaScript
│   └── lib/                    # Third-party libraries
├── 🎨 Styles/                  # Tailwind Source
├── ⚙️ Program.cs               # Application Entry Point
├── 📝 appsettings.json         # Configuration
└── 📦 package.json             # Frontend Dependencies
```

---

## 🎨 Screenshots & Demo

<details>
<summary>📸 Xem ảnh chụp màn hình</summary>

### 🏠 Dashboard
- Tổng quan real-time với biểu đồ
- Metrics quan trọng: Revenue, Members, Classes
- Quick actions và notifications

### 👥 Quản lý người dùng  
- Danh sách thành viên với filters
- Profile management với avatar upload
- Role-based access control

### 🏃‍♂️ Quản lý lớp học
- Class schedule với calendar view
- Booking management
- Trainer assignment

### 📊 Báo cáo & Analytics
- Revenue charts với Chart.js
- Member growth statistics
- Trainer performance metrics

</details>

---

## 🔧 API & Tích hợp

### 🌐 REST Endpoints

| Endpoint | Method | Mô tả |
|----------|--------|-------|
| `/api/nguoidung` | GET | Danh sách người dùng |
| `/api/goitap` | GET/POST | Quản lý gói tập |
| `/api/lophoc` | GET/POST | Quản lý lớp học |
| `/api/diemdanh/face` | POST | Face recognition check-in |
| `/api/thanhtoan/vnpay` | POST | VNPay payment |
| `/api/baocao/dashboard` | GET | Dashboard data |

### 🔌 Webhooks
- VNPay payment notifications
- Email delivery status
- System health checks

---

## 🧪 Testing & Quality

### 🔍 Code Quality
- **87 C# files** với kiến trúc sạch
- **3,142 lines** of controller code
- Repository + Unit of Work pattern
- Dependency Injection throughout

### 🛡️ Security Features
- Password hashing với BCrypt
- HTTPS enforcement
- CSRF protection
- Input validation & sanitization
- Role-based authorization

### ⚡ Performance
- Entity Framework optimization
- Memory caching
- Connection pooling
- Lazy loading
- Image optimization

---

## 🚀 Roadmap & Features sắp tới

### 📅 Version 2.0 (Q2 2024)
- [ ] 📱 Mobile App (React Native)
- [ ] 🤖 AI Workout Recommendations
- [ ] 💬 Real-time Chat Support
- [ ] 📊 Advanced Analytics Dashboard
- [ ] 🔔 Push Notifications

### 📅 Version 2.1 (Q3 2024)  
- [ ] 🌐 Multi-language Support
- [ ] 💳 More Payment Gateways
- [ ] 📈 Member Progress Tracking
- [ ] 🎯 Gamification Features
- [ ] 🔄 API for Third-party Integration

---

## 🤝 Đóng góp

Chúng tôi hoan nghênh mọi đóng góp! 

### 🛠️ Cách đóng góp

1. **Fork** repository
2. **Tạo branch** (`git checkout -b feature/AmazingFeature`)
3. **Commit** changes (`git commit -m 'Add some AmazingFeature'`)
4. **Push** to branch (`git push origin feature/AmazingFeature`)  
5. **Tạo Pull Request**

### 📋 Coding Standards
- Follow C# naming conventions
- Add XML documentation
- Write unit tests cho business logic
- Use meaningful commit messages

---

## 📞 Hỗ trợ & Liên hệ

<div align="center">

| Kênh | Thông tin |
|------|-----------|
| 📧 **Email** | [90683814+daihoangphuc@users.noreply.github.com](mailto:90683814+daihoangphuc@users.noreply.github.com) |
| 🐙 **GitHub** | [@daihoangphuc](https://github.com/daihoangphuc) |
| 🐛 **Issues** | [GitHub Issues](https://github.com/daihoangphuc/qlpg/issues) |
| 💡 **Discussions** | [GitHub Discussions](https://github.com/daihoangphuc/qlpg/discussions) |

### 💬 Community
- 🌟 **Star** nếu project hữu ích!
- 🐛 **Report bugs** qua GitHub Issues
- 💡 **Suggest features** qua Discussions
- 🤝 **Contribute** code hoặc documentation

</div>

---

## 📄 License & Acknowledgments

### 📜 License
Dự án này được phân phối dưới **MIT License**. Xem file [LICENSE](LICENSE) để biết thêm chi tiết.

### 🙏 Acknowledgments & Dependencies

| Technology | Purpose | Version |
|------------|---------|---------|
| [ASP.NET Core](https://docs.microsoft.com/aspnet/core/) | Web Framework | 8.0 |
| [Entity Framework Core](https://docs.microsoft.com/ef/core/) | ORM | 8.0.12 |
| [Tailwind CSS](https://tailwindcss.com/) | CSS Framework | 3.4.0 |
| [Flowbite](https://flowbite.com/) | UI Components | 2.2.0 |
| [Chart.js](https://www.chartjs.org/) | Data Visualization | Latest |
| [BCrypt.Net](https://github.com/BcryptNet/bcrypt.net) | Password Hashing | 4.0.3 |
| [Serilog](https://serilog.net/) | Structured Logging | 8.0.0 |
| [MailKit](https://github.com/jstedfast/MailKit) | Email Client | 4.3.0 |

---

<div align="center">

### 🌟 **Gym Management System** 🌟

**Giải pháp toàn diện cho phòng gym hiện đại**

[![Made with ❤️ in Vietnam](https://img.shields.io/badge/Made%20with%20❤️%20in-Vietnam-red?style=for-the-badge)](https://github.com/daihoangphuc/qlpg)

**⭐ Nếu dự án này hữu ích, hãy cho chúng tôi một star! ⭐**

</div>
