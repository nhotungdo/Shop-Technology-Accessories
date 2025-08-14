<div align="center">

<h1>🛍️ Shop Technology Accessories</h1>

Website bán phụ kiện công nghệ (ASP.NET Core MVC + EF Core + SQL Server)

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-1f6feb)](https://learn.microsoft.com/aspnet/core)
[![EF Core](https://img.shields.io/badge/EF_Core-SqlServer-2f2f2f)](https://learn.microsoft.com/ef/core)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952b3?logo=bootstrap&logoColor=fff)](https://getbootstrap.com/)

<sub>Tip: Nhấn ⭐ ở góc phải nếu repo hữu ích!</sub>

</div>

---

## ✨ Tính năng nổi bật

- ✅ Trang chủ hiện đại, phân trang sản phẩm mới
- 🔎 Tìm kiếm, lọc theo danh mục và khoảng giá
- 👤 Đăng ký/Đăng nhập (Cookie Auth, PBKDF2 hashing)
- 🛒 Giỏ hàng: thêm/cập nhật/xóa, tính tổng
- 💳 Thanh toán demo (PayPal/VNPay – stub)
- 📦 Đơn hàng của tôi: danh sách + chi tiết
- 🛠️ Admin: Dashboard, Quản lý Sản phẩm/Danh mục/Đơn hàng/Người dùng (CRUD, phân quyền `AdminOnly`)

## 🧱 Kiến trúc & Công nghệ

- ASP.NET Core MVC 8
- Entity Framework Core (SqlServer)
- Cookie Authentication + Policy `AdminOnly`
- Services: `PasswordHasher` (PBKDF2), `IPaymentService` (PayPal), `VNPayService`, `IEmailSender`
- UI: Bootstrap 5 + Bootstrap Icons

## 🗂️ Cấu trúc thư mục (rút gọn)

```
Shop-Technology-Accessories/
├─ ShopTechnologyAccessories/
│  ├─ Controllers/                 # Home, Products, Account, Cart, Checkout, Orders
│  ├─ Areas/Admin/                 # Controllers & Views cho Admin
│  ├─ Models/                      # EF Core models & DbContext
│  ├─ Services/                    # PasswordHasher, Payments, Email sender
│  ├─ Views/                       # Razor views
│  ├─ wwwroot/                     # css, js, images
│  └─ Program.cs                   # đăng ký services, route, seed dữ liệu
└─ db/Demo1_ShopTechnologyAccessories.sql  # kịch bản tạo & seed DB mẫu
```

## 🛢️ Cơ sở dữ liệu

- Bảng chính: `Users`, `Roles`, `Categories`, `Products`, `ProductImages`, `Carts`, `CartItems`, `Orders`, `OrderDetails`, `Payments`.
- Kịch bản tạo DB và seed mẫu: `db/Demo1_ShopTechnologyAccessories.sql`.

> Lưu ý: Ứng dụng cũng có `Database.EnsureCreated()` khi khởi động, nhưng để có dữ liệu mẫu phong phú, nên chạy file `.sql` trên SQL Server.

## 🚀 Khởi động nhanh

1) Clone & mở project

```bash
git clone <repo-url>
cd Shop-Technology-Accessories/ShopTechnologyAccessories
```

2) Cấu hình chuỗi kết nối trong `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DBDefault": "Data Source=YOUR_SQL;Initial Catalog=ShopTechnologyAccessories;User ID=sa;Password=***;Trusted_Connection=True;Trust Server Certificate=True"
  }
}
```

3) (Khuyến nghị) Chạy script DB mẫu

- Mở file `db/Demo1_ShopTechnologyAccessories.sql` trong SSMS và Execute.

4) Restore/Build/Run

```bash
dotnet restore
dotnet build
dotnet run
```

Truy cập: `https://localhost:7291`

## 🔐 Tài khoản seed (mặc định)

- Admin A: `admin@shop.local` / `Admin@123`
- Admin B (test): `donhotung2004@gmail.com` / `123456`

> Có thể đổi trong `Program.cs` phần seed.

## 🖼️ Ảnh minh họa

```
docs/screenshots/
├─ home.png
├─ products.png
├─ cart.png
└─ admin-dashboard.png
```

Bạn có thể thêm ảnh thật vào thư mục trên để README hiển thị đẹp hơn.

## 🧭 Roadmap

- [ ] Tích hợp PayPal/VNPay thực (SDK, verify)
- [ ] Upload & quản lý ảnh sản phẩm
- [ ] Xác thực email, quên mật khẩu
- [ ] Báo cáo nâng cao (tháng/quý), sản phẩm bán chạy
- [ ] Unit/Integration tests

## 🤝 Đóng góp

Mọi đóng góp đều được chào đón! Hãy tạo Issue hoặc PR. Vui lòng tuân thủ style code hiện tại.

## 📄 Giấy phép

MIT — sử dụng tự do cho học tập và thương mại (không bảo hành).
