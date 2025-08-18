# 🛍️ Shop Technology Accessories

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-green.svg)](https://dotnet.microsoft.com/apps/aspnet)
[![Entity Framework](https://img.shields.io/badge/Entity%20Framework-8.0-orange.svg)](https://docs.microsoft.com/en-us/ef/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-red.svg)](https://www.microsoft.com/en-us/sql-server)

> **Shop Technology Accessories** là một ứng dụng web thương mại điện tử được xây dựng bằng ASP.NET Core MVC, chuyên cung cấp các phụ kiện công nghệ hiện đại và chất lượng cao.

## 📋 Mục lục

- [Tính năng](#-tính-năng)
- [Công nghệ sử dụng](#-công-nghệ-sử-dụng)
- [Cài đặt](#-cài-đặt)
- [Cấu trúc dự án](#-cấu-trúc-dự-án)
- [Database Schema](#-database-schema)
- [API Endpoints](#-api-endpoints)
- [Hướng dẫn sử dụng](#-hướng-dẫn-sử-dụng)
- [Đóng góp](#-đóng-góp)
- [Giấy phép](#-giấy-phép)

## ✨ Tính năng

### 🔹 1. Chức năng Người dùng (Customer)

#### 👤 Tài khoản & Xác thực
- ✅ Đăng ký tài khoản mới
- ✅ Đăng nhập/Đăng xuất
- ✅ Quản lý thông tin cá nhân (FullName, Email, SĐT, mật khẩu)
- ✅ Quản lý địa chỉ giao hàng

#### 🛒 Mua sắm sản phẩm
- ✅ Xem danh mục sản phẩm (Categories)
- ✅ Xem chi tiết sản phẩm (Products + ProductImages)
- ✅ Tìm kiếm và lọc sản phẩm theo tên, danh mục, giá
- ✅ Bộ lọc nâng cao (theo giá, danh mục, tên, hàng còn/hết)

#### 🛍️ Giỏ hàng (Carts + CartItems)
- ✅ Thêm sản phẩm vào giỏ hàng
- ✅ Cập nhật số lượng sản phẩm trong giỏ
- ✅ Xóa sản phẩm khỏi giỏ
- ✅ Xem tổng giá trị giỏ hàng
- ✅ Xóa toàn bộ giỏ hàng

#### ❤️ Yêu thích (Wishlists)
- ✅ Thêm sản phẩm vào wishlist
- ✅ Xóa sản phẩm khỏi wishlist
- ✅ Xem danh sách sản phẩm yêu thích

#### 📦 Đặt hàng (Orders + OrderDetails)
- ✅ Tạo đơn hàng từ giỏ hàng
- ✅ Nhập địa chỉ giao hàng
- ✅ Chọn phương thức thanh toán (Payments: COD, PayPal, VNPay…)
- ✅ Theo dõi trạng thái đơn hàng (Pending, Paid, Shipped, Completed, Canceled)
- ✅ Xem lịch sử đơn hàng đã mua

### 🔹 2. Chức năng Admin / Quản trị

#### 👥 Quản lý người dùng (Users + Roles)
- ✅ Xem danh sách người dùng
- ✅ Cấp quyền (Admin / Customer)
- ✅ Khóa/mở tài khoản người dùng

#### 📂 Quản lý danh mục (Categories)
- ✅ Thêm, sửa, xóa danh mục sản phẩm

#### 📦 Quản lý sản phẩm (Products + ProductImages)
- ✅ Thêm sản phẩm mới
- ✅ Cập nhật thông tin sản phẩm (tên, mô tả, giá, số lượng)
- ✅ Thêm/xóa hình ảnh sản phẩm
- ✅ Xóa sản phẩm (kèm theo xóa ảnh và giỏ hàng liên quan nhờ ON DELETE CASCADE)

#### 📋 Quản lý đơn hàng (Orders + OrderDetails)
- ✅ Xem danh sách đơn hàng
- ✅ Cập nhật trạng thái đơn hàng (Pending → Paid → Shipped → Completed / Canceled)
- ✅ Quản lý thông tin vận chuyển

#### 💳 Quản lý thanh toán (Payments)
- ✅ Xem danh sách giao dịch thanh toán
- ✅ Cập nhật trạng thái thanh toán (Pending, Success, Failed)

### 🔹 3. Chức năng chung & mở rộng
- ✅ Trang chủ hiển thị sản phẩm nổi bật / mới nhất
- ✅ Thông báo (notification/email) khi đặt hàng thành công hoặc thanh toán thành công
- ✅ Thống kê, báo cáo (cho admin)
  - Doanh thu theo ngày/tháng/năm
  - Số đơn hàng theo trạng thái
  - Top sản phẩm bán chạy

## 🛠️ Công nghệ sử dụng

### Backend
- **ASP.NET Core 8.0** - Framework web
- **Entity Framework Core 8.0** - ORM
- **SQL Server** - Database
- **AutoMapper** - Object mapping
- **JWT Bearer** - Authentication

### Frontend
- **Bootstrap 5** - CSS Framework
- **jQuery** - JavaScript library
- **Font Awesome** - Icons
- **Razor Views** - Template engine

### Tools & Libraries
- **Visual Studio 2022** - IDE
- **Git** - Version control
- **NuGet** - Package manager

## 🚀 Cài đặt

### Yêu cầu hệ thống
- .NET 8.0 SDK
- SQL Server 2019+
- Visual Studio 2022 (khuyến nghị)

### Bước 1: Clone repository
```bash
git clone https://github.com/your-username/Shop-Technology-Accessories.git
cd Shop-Technology-Accessories
```

### Bước 2: Cài đặt database
1. Mở SQL Server Management Studio
2. Chạy script `db/Demo1_ShopTechnologyAccessories.sql`
3. Cập nhật connection string trong `appsettings.json`

### Bước 3: Cài đặt dependencies
```bash
cd ShopTechnology
dotnet restore
```

### Bước 4: Chạy ứng dụng
```bash
dotnet run
```

Truy cập: `https://localhost:5001` hoặc `http://localhost:5000`

## 📁 Cấu trúc dự án

```
ShopTechnology/
├── Controllers/           # Controllers cho MVC
│   ├── HomeController.cs
│   ├── AccountController.cs
│   ├── ProductController.cs
│   ├── CartController.cs
│   └── OrderController.cs
├── Models/               # Entity models
│   ├── User.cs
│   ├── Product.cs
│   ├── Category.cs
│   ├── Cart.cs
│   ├── Order.cs
│   └── ShopTechnologyAccessoriesContext.cs
├── ViewModels/           # View models
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   ├── ProductViewModel.cs
│   └── CartViewModel.cs
├── Services/             # Business logic services
│   ├── IProductService.cs
│   └── ProductService.cs
├── Views/                # Razor views
│   ├── Home/
│   ├── Account/
│   ├── Product/
│   └── Cart/
├── wwwroot/              # Static files
│   ├── css/
│   ├── js/
│   └── lib/
└── Areas/                # Admin area
    └── Admin/
        ├── Controllers/
        └── Views/
```

## 🗄️ Database Schema

### Bảng chính
- **Users** - Thông tin người dùng
- **Roles** - Vai trò người dùng
- **Categories** - Danh mục sản phẩm
- **Products** - Sản phẩm
- **ProductImages** - Hình ảnh sản phẩm
- **Carts** - Giỏ hàng
- **CartItems** - Chi tiết giỏ hàng
- **Orders** - Đơn hàng
- **OrderDetails** - Chi tiết đơn hàng
- **Payments** - Thanh toán
- **Wishlists** - Danh sách yêu thích

### Quan hệ
- User ↔ Role (Many-to-One)
- Product ↔ Category (Many-to-One)
- Product ↔ ProductImage (One-to-Many)
- User ↔ Cart (One-to-One)
- Cart ↔ CartItem (One-to-Many)
- User ↔ Order (One-to-Many)
- Order ↔ OrderDetail (One-to-Many)
- Order ↔ Payment (One-to-One)
- User ↔ Wishlist (One-to-Many)

## 🔌 API Endpoints

### Authentication
- `POST /Account/Login` - Đăng nhập
- `POST /Account/Register` - Đăng ký
- `GET /Account/Logout` - Đăng xuất

### Products
- `GET /Product` - Danh sách sản phẩm
- `GET /Product/Details/{id}` - Chi tiết sản phẩm
- `POST /Product/Search` - Tìm kiếm sản phẩm
- `POST /Product/Filter` - Lọc sản phẩm

### Cart
- `GET /Cart` - Xem giỏ hàng
- `POST /Cart/AddToCart` - Thêm vào giỏ hàng
- `POST /Cart/UpdateQuantity` - Cập nhật số lượng
- `POST /Cart/RemoveFromCart` - Xóa khỏi giỏ hàng
- `POST /Cart/ClearCart` - Xóa toàn bộ giỏ hàng

### Orders
- `GET /Order/Checkout` - Trang thanh toán
- `POST /Order/PlaceOrder` - Đặt hàng
- `GET /Order/History` - Lịch sử đơn hàng

## 📖 Hướng dẫn sử dụng

### Cho người dùng
1. **Đăng ký/Đăng nhập**: Tạo tài khoản mới hoặc đăng nhập
2. **Mua sắm**: Duyệt sản phẩm, tìm kiếm, lọc theo danh mục/giá
3. **Giỏ hàng**: Thêm sản phẩm, cập nhật số lượng, xóa sản phẩm
4. **Đặt hàng**: Chọn phương thức thanh toán, nhập địa chỉ giao hàng
5. **Theo dõi**: Xem trạng thái đơn hàng và lịch sử mua hàng

### Cho Admin
1. **Quản lý sản phẩm**: Thêm, sửa, xóa sản phẩm và hình ảnh
2. **Quản lý đơn hàng**: Cập nhật trạng thái, xử lý thanh toán
3. **Quản lý người dùng**: Xem danh sách, cấp quyền
4. **Thống kê**: Xem báo cáo doanh thu, đơn hàng

## 🤝 Đóng góp

1. Fork dự án
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit thay đổi (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Mở Pull Request

## 📄 Giấy phép

Dự án này được phân phối dưới giấy phép MIT. Xem `LICENSE` để biết thêm thông tin.

## 📞 Liên hệ

- **Email**: donhotung2004@gmail.com
- **Phone**: 0931982568
- **Website**: [Shop Technology Accessories](https://shoptechnology.com)

---

⭐ Nếu dự án này hữu ích, hãy cho chúng tôi một star!
