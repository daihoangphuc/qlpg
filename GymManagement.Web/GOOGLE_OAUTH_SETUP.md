# 🔐 Hướng dẫn tích hợp Google OAuth 2.0

## 📋 Tổng quan

Dự án Gym Management System đã được tích hợp Google OAuth 2.0 để cho phép người dùng đăng nhập bằng tài khoản Google. Tính năng này giúp:

- ✅ **Đơn giản hóa quá trình đăng nhập**
- ✅ **Tăng tính bảo mật** (không cần nhớ mật khẩu)
- ✅ **Tự động tạo tài khoản** cho người dùng mới
- ✅ **Link với tài khoản hiện có** nếu email đã tồn tại

## 🚀 Cách setup Google OAuth

### 1. **Tạo Google Cloud Project**

1. Truy cập [Google Cloud Console](https://console.cloud.google.com/)
2. Tạo project mới hoặc chọn project hiện có
3. Kích hoạt Google+ API

### 2. **Tạo OAuth 2.0 Credentials**

1. Vào **APIs & Services** > **Credentials**
2. Click **Create Credentials** > **OAuth 2.0 Client IDs**
3. Chọn **Web application**
4. Điền thông tin:
   - **Name**: Gym Management System
   - **Authorized JavaScript origins**:
     - `http://localhost:5000`
     - `https://yourdomain.com` (production)
   - **Authorized redirect URIs**:
     - `http://localhost:5000/Auth/GoogleCallback`
     - `https://yourdomain.com/Auth/GoogleCallback` (production)

### 3. **Lấy Client ID và Client Secret**

Sau khi tạo, bạn sẽ nhận được:
- **Client ID**: `your-project-id.apps.googleusercontent.com`
- **Client Secret**: `GOCSPX-xxxxxxxxxxxxxxxx`

### 4. **Cập nhật cấu hình**

Cập nhật file `appsettings.json`:

```json
{
  "GoogleAuth": {
    "ClientId": "your-project-id.apps.googleusercontent.com",
    "ClientSecret": "GOCSPX-xxxxxxxxxxxxxxxx"
  }
}
```

### 5. **Cấu hình cho Production**

Khi deploy lên production, cần:

1. **Cập nhật redirect URIs** trong Google Cloud Console
2. **Sử dụng HTTPS** cho production
3. **Cập nhật appsettings.Production.json**:

```json
{
  "GoogleAuth": {
    "ClientId": "your-production-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-production-client-secret"
  }
}
```

## 🔧 Kiến trúc tích hợp

### **Services**

- **IGoogleAuthService**: Interface cho Google authentication
- **GoogleAuthService**: Implementation xử lý logic Google auth
- **ExternalLogin Model**: Lưu trữ thông tin Google account

### **Controllers**

- **AuthController.GoogleLogin()**: Khởi tạo Google OAuth flow
- **AuthController.GoogleCallback()**: Xử lý callback từ Google

### **Database**

```sql
-- Bảng ExternalLogin đã có sẵn
CREATE TABLE ExternalLogin(
    Id NVARCHAR(450) PRIMARY KEY,
    TaiKhoanId NVARCHAR(450) NOT NULL,
    Provider NVARCHAR(100) NOT NULL, -- "Google"
    ProviderKey NVARCHAR(200) NOT NULL, -- Google User ID
    ProviderDisplayName NVARCHAR(200),
    NgayTao DATETIME2 DEFAULT GETDATE()
);
```

## 🎯 Tính năng

### **Đăng nhập Google**
- ✅ Tự động tạo tài khoản mới nếu email chưa tồn tại
- ✅ Link với tài khoản hiện có nếu email đã tồn tại
- ✅ Gán role "Member" mặc định cho user mới
- ✅ Tạo session và redirect về trang chính

### **Bảo mật**
- ✅ Sử dụng OAuth 2.0 protocol
- ✅ HTTPS cho production
- ✅ Secure cookie settings
- ✅ CSRF protection

### **User Experience**
- ✅ Nút "Đăng nhập với Google" đẹp mắt
- ✅ Loading states và error handling
- ✅ Responsive design
- ✅ Vietnamese localization

## 🧪 Testing

### **Test trong Development**

1. **Chạy ứng dụng**: `dotnet run`
2. **Truy cập**: `http://localhost:5000/Auth/Login`
3. **Click nút Google**: Kiểm tra redirect flow
4. **Kiểm tra database**: Xem ExternalLogin records

### **Test Cases**

- ✅ **New user**: Tạo tài khoản mới
- ✅ **Existing user**: Link với tài khoản hiện có
- ✅ **Invalid credentials**: Error handling
- ✅ **Network issues**: Timeout handling

## 🔍 Troubleshooting

### **Lỗi thường gặp**

1. **"Invalid redirect URI"**
   - Kiểm tra redirect URI trong Google Cloud Console
   - Đảm bảo protocol (http/https) đúng

2. **"Client ID not found"**
   - Kiểm tra Client ID trong appsettings.json
   - Đảm bảo project đã được kích hoạt

3. **"Access denied"**
   - Kiểm tra Google+ API đã được enable
   - Kiểm tra OAuth consent screen

### **Debug Tips**

```csharp
// Thêm logging để debug
_logger.LogInformation("Google callback received for email: {Email}", email);
_logger.LogInformation("Google ID: {GoogleId}", googleId);
```

## 📚 Tài liệu tham khảo

- [Google OAuth 2.0 Documentation](https://developers.google.com/identity/protocols/oauth2)
- [ASP.NET Core Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [Google Cloud Console](https://console.cloud.google.com/)

## 🚀 Deployment Checklist

- [ ] Cập nhật redirect URIs cho production domain
- [ ] Sử dụng HTTPS cho production
- [ ] Cập nhật appsettings.Production.json
- [ ] Test Google login flow trên production
- [ ] Kiểm tra logging và monitoring
- [ ] Backup database trước khi deploy

---

**Lưu ý**: Đảm bảo bảo mật Client Secret và không commit vào source code!