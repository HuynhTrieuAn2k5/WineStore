# Chilliquer ASP.NET Core MVC + SQL Server

Bộ scaffold này được chuyển từ database Supabase/PostgreSQL sang SQL Server để chạy trên SSMS và ASP.NET Core MVC.

## 1. Tạo database bằng SSMS

Mở SSMS, chạy file:

```sql
Database/Chilliquer_SqlServer_SSMS.sql
```

Database tạo ra: `ChilliquerDB`.

Tài khoản mẫu:

```txt
Username: admin
Password: Admin@123
```

Các tài khoản khác: `quanly`, `banhang1`, `banhang2`, mật khẩu đều là `Admin@123`.

Bản này **không có vai trò Thủ kho**. Tất cả nghiệp vụ kho như nhập kho, xem lô hàng, cảnh báo tồn kho và chỉnh thông tin sản phẩm do `admin` hoặc `manager` xử lý. Nhân viên bán hàng chỉ xem sản phẩm và thực hiện các nghiệp vụ bán hàng.

## 2. Chạy project

Sửa chuỗi kết nối trong `appsettings.json` nếu SQL Server của bạn không phải `Server=.`.

```bash
dotnet restore
dotnet run
```

## 3. Kiến trúc code

```txt
Controllers/        Xử lý request MVC
Data/               DbContext EF Core
Models/Entities/    Entity map với bảng SQL Server
ViewModels/         Dữ liệu trung gian cho form
Views/              Razor UI
Services/           PasswordService, CurrentUserService
Filters/            RequireLogin, RequireRole
wwwroot/css/        Theme đỏ rượu vang + vàng đồng
Database/           Script SSMS
```

## 4. Lý do chỉnh database

- Supabase dùng `auth.users`, RLS, PostgreSQL function/trigger nên không chạy trực tiếp trên SSMS.
- ASP.NET Core MVC ở bản này dùng `UserAccounts`, `Roles`, `Session`, `PBKDF2 password hash`.
- Phân quyền chính: `admin` toàn quyền, `manager` quản lý bán hàng kiêm kho, `sales` bán hàng, `customer` dự phòng cho cổng khách hàng nếu phát triển sau.
- Các bảng nghiệp vụ chính vẫn giữ: sản phẩm, kho, khách hàng, thẻ thành viên, hóa đơn, đặt cọc, đổi/trả, báo cáo.
